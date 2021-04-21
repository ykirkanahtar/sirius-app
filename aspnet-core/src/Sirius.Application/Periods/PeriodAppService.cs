using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sirius.Housings;
using Sirius.PaymentAccounts;
using Sirius.PaymentCategories;
using Sirius.Periods.Dto;
using Sirius.Shared.Dtos;
using Sirius.Shared.Helper;

namespace Sirius.Periods
{
    public class PeriodAppService :
        AsyncCrudAppService<Period, PeriodDto, Guid, PagedPeriodResultRequestDto, CreatePeriodForSiteDto,
            UpdatePeriodDto>,
        IPeriodAppService
    {
        private readonly IRepository<Period, Guid> _periodRepository;
        private readonly IPeriodManager _periodManager;
        private readonly IBlockManager _blockManager;
        private readonly IAccountBookManager _accountBookManager;
        private readonly IRepository<PaymentCategory, Guid> _paymentCategoryRepository;
        private readonly IRepository<AccountBook, Guid> _accountBookRepository;
        private readonly IRepository<PaymentAccount, Guid> _paymentAccountRepository;
        private readonly IAccountBookPolicy _accountBookPolicy;
        private readonly IBalanceOrganizer _balanceOrganizer;

        public PeriodAppService(IRepository<Period, Guid> periodRepository, IPeriodManager periodManager,
            IBlockManager blockManager, IRepository<PaymentCategory, Guid> paymentCategoryRepository,
            IRepository<AccountBook, Guid> accountBookRepository,
            IRepository<PaymentAccount, Guid> paymentAccountRepository,
            IAccountBookManager accountBookManager,
            IAccountBookPolicy accountBookPolicy, IBalanceOrganizer balanceOrganizer) : base(
            periodRepository)
        {
            _periodRepository = periodRepository;
            _periodManager = periodManager;
            _blockManager = blockManager;
            _paymentCategoryRepository = paymentCategoryRepository;
            _accountBookRepository = accountBookRepository;
            _paymentAccountRepository = paymentAccountRepository;
            _accountBookManager = accountBookManager;
            _accountBookPolicy = accountBookPolicy;
            _balanceOrganizer = balanceOrganizer;
        }

        public async Task<PeriodDto> CreateForSiteAsync(CreatePeriodForSiteDto input)
        {
            CheckCreatePermission();

            var period = Period.CreateForSite(
                SequentialGuidGenerator.Instance.Create()
                , AbpSession.GetTenantId()
                , input.Name
                , input.StartDateString.StringToDateTime()
                , input.EndDateString.StringToNullableDateTime()
            );

            await CreateAsync(period, input.PaymentCategories);

            return ObjectMapper.Map<PeriodDto>(period);
        }

        public async Task<PeriodDto> CreateForBlockAsync(CreatePeriodForBlockDto input)
        {
            CheckCreatePermission();

            var block = await _blockManager.GetAsync(input.BlockId);

            var period = Period.CreateForBlock(
                SequentialGuidGenerator.Instance.Create()
                , AbpSession.GetTenantId()
                , input.Name
                , input.StartDateString.StringToDateTime()
                , input.EndDateString.StringToNullableDateTime()
                , block
            );

            await CreateAsync(period, input.PaymentCategories);

            return ObjectMapper.Map<PeriodDto>(period);
        }

        private async Task CreateAsync(Period period, List<Guid> paymentCategories)
        {
            //Yeni başlayacak dönemden daha yeni dönem varsa hata fırlatılıyor
            var newerPeriods = await _periodRepository.GetAll()
                .Where(p => p.SiteOrBlock == period.SiteOrBlock && p.StartDate >= period.StartDate).ToListAsync();

            if (newerPeriods.Any())
            {
                throw new UserFriendlyException("Sistemde daha yeni tarihli dönem bulunmaktadır.");
            }

            var existingAccountBooksInNewPeriod = await _accountBookRepository.GetAll()
                .Where(p => p.ProcessDateTime > period.StartDate).CountAsync();
            if (existingAccountBooksInNewPeriod > 0)
            {
                throw new UserFriendlyException(
                    "Bu dönemi kapsayan eski dönem hareketleri vardır, lütfen dönem tarihini değiştiriniz.");
            }

            var activePeriod = await _periodManager.GetActivePeriod(false);

            if (activePeriod != null) //Önceki dönemden devir işlemleri yapılıyor
            {
                var paymentAccounts = await _paymentAccountRepository.GetAll().Where(p => p.Balance != 0).ToListAsync();
                foreach (var paymentAccount in paymentAccounts)
                {
                    var balance = paymentAccount.Balance;

                    var sameDayIndex = await _accountBookManager.GetSameDayIndexAsync(period.StartDate);

                    var accountBookTransferForOldPeriod =
                        await AccountBook.CreateTransferToNextPeriodForPaymentAccountAsync(
                            _accountBookPolicy
                            , SequentialGuidGenerator.Instance.Create()
                            , period.TenantId
                            , activePeriod.Id
                            , period.StartDate
                            , paymentAccount
                            , balance
                            , "Sonraki döneme devir"
                            , new List<AccountBookFile>()
                            , AbpSession.GetUserId()
                        );

                    var newIndex = sameDayIndex + 1;
                    accountBookTransferForOldPeriod.SetSameDayIndexManually(newIndex);
                    accountBookTransferForOldPeriod.SetToPaymentAccountCurrentBalance(0);

                    await _accountBookManager.CreateTransferForPaymentAccountAsync(accountBookTransferForOldPeriod,
                        paymentAccount, false);

                    await _balanceOrganizer.GetOrganizedAccountBooksAsync(
                        accountBookTransferForOldPeriod.ProcessDateTime,
                        new List<PaymentAccount> {paymentAccount},
                        new List<AccountBook> {accountBookTransferForOldPeriod}, null, null);
                    _balanceOrganizer.OrganizeAccountBookBalances();

                    var accountBookTransferForNewPeriod =
                        await AccountBook.CreateTransferFromPreviousPeriodForPaymentAccountAsync(
                            _accountBookPolicy
                            , SequentialGuidGenerator.Instance.Create()
                            , period.TenantId
                            , period.Id
                            , period.StartDate
                            , paymentAccount
                            , balance
                            , "Önceki dönemden devir"
                            , new List<AccountBookFile>()
                            , AbpSession.GetUserId()
                        );

                    newIndex = newIndex + 1;
                    accountBookTransferForNewPeriod.SetSameDayIndexManually(newIndex);
                    accountBookTransferForNewPeriod.SetToPaymentAccountCurrentBalance(balance);

                    await _accountBookManager.CreateTransferForPaymentAccountAsync(accountBookTransferForNewPeriod,
                        paymentAccount,
                        false);
                }
            }

            activePeriod?.ClosePeriod(period.StartDate);

            await _periodManager.CreateAsync(period);
            await SetPassivePaymentCategoriesAsync(paymentCategories);
        }

        private async Task SetPassivePaymentCategoriesAsync(List<Guid> activePaymentCategories)
        {
            var allPaymentCategories = await _paymentCategoryRepository.GetAll()
                .Where(p => p.IsActive && p.IsValidForAllPeriods == false &&
                            activePaymentCategories.Contains(p.Id) == false).ToListAsync();

            foreach (var paymentCategory in allPaymentCategories)
            {
                paymentCategory.SetPassive();
            }
        }

        public override Task<PeriodDto> CreateAsync(CreatePeriodForSiteDto input)
        {
            CheckCreatePermission();
            throw new NotSupportedException();
        }

        public override async Task<PeriodDto> UpdateAsync(UpdatePeriodDto input)
        {
            CheckUpdatePermission();
            var existingPeriod = await _periodManager.GetAsync(input.Id);
            var period = Period.Update(existingPeriod, input.Name, input.StartDateString.StringToDateTime(),
                input.EndDateString.StringToNullableDateTime());
            await _periodManager.UpdateAsync(period);
            return ObjectMapper.Map<PeriodDto>(period);
        }

        public override Task DeleteAsync(EntityDto<Guid> input)
        {
            CheckDeletePermission();
            throw new NotSupportedException();
        }

        public override async Task<PagedResultDto<PeriodDto>> GetAllAsync(PagedPeriodResultRequestDto input)
        {
            CheckGetAllPermission();
            var query = _periodRepository.GetAll().Where(p => p.SiteOrBlock == input.SiteOrBlock)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Name),
                    p => p.Name == input.Name)
                .WhereIf(input.BlockId.HasValue,
                    p => p.BlockId == input.BlockId.Value);

            var periods = await query
                .OrderBy(input.Sorting ?? $"{nameof(PeriodDto.Name)}")
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<PeriodDto>(query.Count(),
                ObjectMapper.Map<List<PeriodDto>>(periods));
        }

        public async Task<List<LookUpDto>> GetPeriodLookUpAsync()
        {
            CheckGetAllPermission();

            var period = await _periodRepository.GetAllListAsync();

            return
                (from l in period
                    select new LookUpDto(l.Id.ToString(), l.Name)).ToList();
        }
    }
}