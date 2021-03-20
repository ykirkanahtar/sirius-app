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
using Abp.Localization;
using Abp.Localization.Sources;
using Abp.Runtime.Session;
using Microsoft.EntityFrameworkCore;
using Sirius.PaymentAccounts;
using Sirius.PaymentCategories.Dto;
using Sirius.Shared.Constants;
using Sirius.Shared.Dtos;
using Sirius.Shared.Enums;

namespace Sirius.PaymentCategories
{
    public class PaymentCategoryAppService :
        AsyncCrudAppService<PaymentCategory, PaymentCategoryDto, Guid, PagedPaymentCategoryResultRequestDto,
            CreatePaymentCategoryDto, UpdatePaymentCategoryDto>, IPaymentCategoryAppService
    {
        private readonly IPaymentCategoryManager _paymentCategoryManager;
        private readonly IRepository<PaymentCategory, Guid> _paymentCategoryRepository;
        private readonly IRepository<PaymentAccount, Guid> _paymentAccountRepository;
        private readonly ILocalizationSource _localizationSource;

        public PaymentCategoryAppService(
            IPaymentCategoryManager paymentCategoryManager,
            IRepository<PaymentCategory, Guid> paymentCategoryRepository,
            ILocalizationManager localizationManager,
            IRepository<PaymentAccount, Guid> paymentAccountRepository)
            : base(paymentCategoryRepository)
        {
            _paymentCategoryManager = paymentCategoryManager;
            _paymentCategoryRepository = paymentCategoryRepository;
            _paymentAccountRepository = paymentAccountRepository;
            _localizationSource = localizationManager.GetSource(AppConstants.LocalizationSourceName);
        }

        public async Task<PaymentCategoryDto> CreateIncomeAsync(CreateIncomePaymentCategoryDto input)
        {
            CheckCreatePermission();

            var paymentCategory = PaymentCategory.CreateIncome(SequentialGuidGenerator.Instance.Create(),
                AbpSession.GetTenantId(), input.PaymentCategoryName, /*input.HousingDueType,*/ input.IsValidForAllPeriods,
                input.DefaultToPaymentAccountId);

            await _paymentCategoryManager.CreateAsync(paymentCategory);
            return ObjectMapper.Map<PaymentCategoryDto>(paymentCategory);
        }

        public async Task<PaymentCategoryDto> CreateExpenseAsync(CreateExpensePaymentCategoryDto input)
        {
            CheckCreatePermission();

            var paymentCategory = PaymentCategory.CreateExpense(SequentialGuidGenerator.Instance.Create(),
                AbpSession.GetTenantId(), input.PaymentCategoryName, input.IsValidForAllPeriods,
                input.DefaultFromPaymentAccountId);

            await _paymentCategoryManager.CreateAsync(paymentCategory);
            return ObjectMapper.Map<PaymentCategoryDto>(paymentCategory);
        }

        public async Task<PaymentCategoryDto> CreateTransferAsync(CreateTransferPaymentCategoryDto input)
        {
            CheckCreatePermission();

            var paymentCategory = PaymentCategory.CreateTransferBetweenAccounts(SequentialGuidGenerator.Instance.Create(),
                AbpSession.GetTenantId(), input.PaymentCategoryName, /*null, */input.IsValidForAllPeriods,
                input.DefaultFromPaymentAccountId, input.DefaultToPaymentAccountId);

            await _paymentCategoryManager.CreateAsync(paymentCategory);
            return ObjectMapper.Map<PaymentCategoryDto>(paymentCategory);
        }

        public override async Task<PaymentCategoryDto> CreateAsync(CreatePaymentCategoryDto input)
        {
            throw new NotSupportedException();
            // CheckCreatePermission();
            // if (input.HousingDueType == HousingDueType.RegularHousingDue)
            // {
            //     throw new UserFriendlyException("Geçersiz aidat ödemesi tipi");
            // }
            //
            // var paymentCategory = PaymentCategory.Create(SequentialGuidGenerator.Instance.Create(),
            //     AbpSession.GetTenantId(), input.PaymentCategoryName, input.HousingDueType, input.IsValidForAllPeriods,
            //     input.DefaultFromPaymentAccountId, input.DefaultToPaymentAccountId);
            // await _paymentCategoryManager.CreateAsync(paymentCategory);
            // return ObjectMapper.Map<PaymentCategoryDto>(paymentCategory);
        }

        public override async Task<PaymentCategoryDto> UpdateAsync(UpdatePaymentCategoryDto input)
        {
            CheckUpdatePermission();
            var existingPaymentCategory = await _paymentCategoryManager.GetAsync(input.Id);
            var paymentCategory = PaymentCategory.Update(existingPaymentCategory, input.PaymentCategoryName,
                input.DefaultFromPaymentAccountId, input.DefaultToPaymentAccountId);
            await _paymentCategoryManager.UpdateAsync(paymentCategory);
            return ObjectMapper.Map<PaymentCategoryDto>(paymentCategory);
        }

        public override async Task DeleteAsync(EntityDto<Guid> input)
        {
            CheckDeletePermission();
            var paymentCategory = await _paymentCategoryManager.GetAsync(input.Id);
            await _paymentCategoryManager.DeleteAsync(paymentCategory);
        }

        public override async Task<PagedResultDto<PaymentCategoryDto>> GetAllAsync(
            PagedPaymentCategoryResultRequestDto input)
        {
            CheckGetAllPermission();

            var query = from pc in _paymentCategoryRepository.GetAll()
                join fpa in _paymentAccountRepository.GetAll() on pc.DefaultFromPaymentAccountId equals fpa.Id into fpa
                from subFpa in fpa.DefaultIfEmpty()
                join tpa in _paymentAccountRepository.GetAll() on pc.DefaultToPaymentAccountId equals tpa.Id into tpa
                from subTpa in tpa.DefaultIfEmpty()
                where pc.IsActive
                select new PaymentCategoryDto
                {
                    Id = pc.Id,
                    PaymentCategoryName = pc.PaymentCategoryName,
                    IsHousingDue = pc.IsHousingDue,
                    IsValidForAllPeriods = pc.IsValidForAllPeriods,
                    DefaultFromPaymentAccountId = pc.DefaultFromPaymentAccountId,
                    DefaultToPaymentAccountId = pc.DefaultToPaymentAccountId,
                    DefaultFromPaymentAccountName = subFpa != null ? subFpa.AccountName : string.Empty,
                    DefaultToPaymentAccountName = subTpa != null ? subTpa.AccountName : string.Empty
                };

            query = query
                .WhereIf(!string.IsNullOrWhiteSpace(input.PaymentCategoryName),
                    p => p.PaymentCategoryName.Contains(input.PaymentCategoryName));

            var paymentCategories = await query
                .OrderBy(input.Sorting ?? $"{nameof(PaymentCategoryDto.PaymentCategoryName)}")
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<PaymentCategoryDto>(query.Count(), paymentCategories);
        }

        //silinecek
        // public async Task<PaymentCategoryDto> GetRegularHousingDueAsync()
        // {
        //     CheckGetAllPermission();
        //
        //     var paymentCategory = await _paymentCategoryManager.GetRegularHousingDueAsync();
        //     return ObjectMapper.Map<PaymentCategoryDto>(paymentCategory);
        // }

        public async Task<List<PaymentCategoryDto>> GetPaymentCategoryForMenuAsync()
        {
            CheckGetAllPermission();

            var paymentCategories = await _paymentCategoryRepository.GetAll()
                .Where(p => p.IsActive)
                .OrderBy(p => p.PaymentCategoryType)
                .ToListAsync();

            return ObjectMapper.Map<List<PaymentCategoryDto>>(paymentCategories);
        }
        
        public async Task<List<LookUpDto>> GetLookUp(bool onlyActives)
        {
            CheckGetAllPermission();

            var paymentCategories = await _paymentCategoryRepository.GetAll()
                .WhereIf(onlyActives, p => p.IsActive)
                .ToListAsync();

            return
                (from l in paymentCategories.OrderBy(p => _localizationSource.GetString(p.PaymentCategoryName))
                    select new LookUpDto(l.Id.ToString(),
                        _localizationSource.GetString(l.PaymentCategoryName)))
                .ToList();
        }

        public async Task<List<LookUpDto>> GetLookUpByPaymentCategoryType(bool onlyActives, PaymentCategoryType paymentCategoryType)
        {
            CheckGetAllPermission();

            var paymentCategories = await _paymentCategoryRepository.GetAll()
                .WhereIf(onlyActives, p => p.IsActive && p.PaymentCategoryType == paymentCategoryType)
                .ToListAsync();

            return
                (from l in paymentCategories.OrderBy(p => _localizationSource.GetString(p.PaymentCategoryName))
                    select new LookUpDto(l.Id.ToString(),
                        _localizationSource.GetString(l.PaymentCategoryName)))
                .ToList();
        }

        //Önceki dönemden devredilecek ödeme kategorileri çekiliyor
        public async Task<List<LookUpDto>> GetPaymentCategoryForTransferLookUpAsync()
        {
            CheckGetAllPermission();

            var paymentCategories = await _paymentCategoryRepository.GetAll()
                .Where(p => p.IsActive && p.IsValidForAllPeriods == false)
                .ToListAsync();

            return
                (from l in paymentCategories.OrderBy(p => _localizationSource.GetString(p.PaymentCategoryName))
                    select new LookUpDto(l.Id.ToString(),
                        _localizationSource.GetString(l.PaymentCategoryName)))
                .ToList();
        }

        //silinecek
        // public async Task<List<LookUpDto>> GetHousingDuePaymentCategoryLookUpAsync(bool onlyActives)
        // {
        //     CheckGetAllPermission();
        //
        //     var paymentAccounts = await _paymentCategoryRepository.GetAll()
        //         .Where(p => p.HousingDueType == HousingDueType.RegularHousingDue ||
        //                     p.HousingDueType == HousingDueType.AdditionalHousingDueForOwner ||
        //                     p.HousingDueType == HousingDueType.AdditionalHousingDueForResident)
        //         .WhereIf(onlyActives, p => p.IsActive)
        //         .ToListAsync();
        //
        //     return
        //         (from l in paymentAccounts.OrderBy(p => _localizationSource.GetString(p.PaymentCategoryName))
        //             select new LookUpDto(l.Id.ToString(),
        //                 _localizationSource.GetString(l.PaymentCategoryName)))
        //         .ToList();
        // }

        public async Task<List<string>> GetPaymentCategoryFromAutoCompleteFilterAsync(string request)
        {
            CheckGetAllPermission();

            var query = from p in _paymentCategoryRepository.GetAll()
                where p.PaymentCategoryName.Contains(request)
                select _localizationSource.GetString(p.PaymentCategoryName);

            return await query.ToListAsync();
        }
    }
}