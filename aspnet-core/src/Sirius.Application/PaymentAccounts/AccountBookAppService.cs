﻿using System;
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
using Abp.UI;
using Azure.Storage.Blobs.Models;
using Microsoft.EntityFrameworkCore;
using Sirius.AccountBooks.Dto;
using Sirius.EntityFrameworkCore.Repositories;
using Sirius.FileServices;
using Sirius.HousingPaymentPlans;
using Sirius.Housings;
using Sirius.PaymentAccounts.Dto;
using Sirius.PaymentCategories;
using Sirius.PaymentCategories.Dto;
using Sirius.Shared.Constants;
using Sirius.Shared.Dtos;
using Sirius.Shared.Enums;

namespace Sirius.PaymentAccounts
{
    public class AccountBookAppService :
        AsyncCrudAppService<AccountBook, AccountBookDto, Guid, PagedAccountBookResultRequestDto, CreateAccountBookDto,
            UpdateAccountBookDto>, IAccountBookAppService
    {
        private readonly IAccountBookManager _accountBookManager;
        private readonly IRepository<AccountBook, Guid> _accountBookRepository;
        private readonly IRepository<PaymentCategory, Guid> _paymentCategoryRepository;
        private readonly IHousingRepository _housingRepository;
        private readonly IRepository<Block, Guid> _blockRepository;
        private readonly IRepository<PaymentAccount, Guid> _paymentAccountRepository;
        private readonly IRepository<AccountBookFile, Guid> _accountBookFileRepository;
        private readonly IPaymentCategoryManager _paymentCategoryManager;
        private readonly IHousingManager _housingManager;
        private readonly IBlobService _blobService;
        private readonly IAccountBookPolicy _accountBookPolicy;
        private readonly IRepository<HousingPaymentPlan, Guid> _housingPaymetPlanRepository;
        private readonly IRepository<HousingPaymentPlanGroup, Guid> _housingPaymentPlanGroupRepository;
        private readonly IHousingPaymentPlanManager _housingPaymentPlanManager;
        private readonly ILocalizationSource _localizationSource;
        private readonly IBalanceOrganizer _balanceOrganizer;

        public AccountBookAppService(IAccountBookManager accountBookManager,
            IRepository<AccountBook, Guid> accountBookRepository,
            IPaymentCategoryManager paymentCategoryManager,
            IHousingManager housingManager,
            IRepository<PaymentCategory, Guid> paymentCategoryRepository,
            IHousingRepository housingRepository,
            IRepository<Block, Guid> blockRepository,
            IRepository<PaymentAccount, Guid> paymentAccountRepository,
            IBlobService blobService,
            IAccountBookPolicy accountBookPolicy,
            IRepository<AccountBookFile, Guid> accountBookFileRepository,
            IRepository<HousingPaymentPlan, Guid> housingPaymetPlanRepository,
            IHousingPaymentPlanManager housingPaymentPlanManager,
            ILocalizationManager localizationManager,
            IRepository<HousingPaymentPlanGroup, Guid> housingPaymentPlanGroupRepository, 
            IBalanceOrganizer balanceOrganizer)
            : base(accountBookRepository)
        {
            _accountBookManager = accountBookManager;
            _accountBookRepository = accountBookRepository;
            _paymentCategoryManager = paymentCategoryManager;
            _housingManager = housingManager;
            _paymentCategoryRepository = paymentCategoryRepository;
            _housingRepository = housingRepository;
            _blockRepository = blockRepository;
            _paymentAccountRepository = paymentAccountRepository;
            _blobService = blobService;
            _accountBookPolicy = accountBookPolicy;
            _accountBookFileRepository = accountBookFileRepository;
            _housingPaymetPlanRepository = housingPaymetPlanRepository;
            _housingPaymentPlanManager = housingPaymentPlanManager;
            _housingPaymentPlanGroupRepository = housingPaymentPlanGroupRepository;
            _balanceOrganizer = balanceOrganizer;
            _localizationSource = localizationManager.GetSource(AppConstants.LocalizationSourceName);
        }

        public override async Task<AccountBookDto> CreateAsync(CreateAccountBookDto input)
        {
            CheckCreatePermission();
            if (input.IsHousingDue)
            {
                var housingDueAccountBook = await CreateHousingDueAsync(input, true);
                return ObjectMapper.Map<AccountBookDto>(housingDueAccountBook);
            }

            var accountBook = await CreateOtherPaymentAsync(input, true);
            return ObjectMapper.Map<AccountBookDto>(accountBook);
        }

        private async Task<AccountBook> CreateHousingDueAsync(CreateAccountBookDto input, bool organizeBalances)
        {
            CheckCreatePermission();
            input.ProcessDateTime = input.ProcessDateTime.Date + new TimeSpan(0, 0, 0);

            var housingDuePaymentCategory = await _paymentCategoryManager.GetAsync(input.PaymentCategoryId);
            var housing = await _housingManager.GetAsync(input.HousingId);

            var housingExistsInHousingPaymentPlanGroup = await _housingPaymentPlanGroupRepository.GetAll()
                .Include(p => p.HousingPaymentPlanGroupHousingCategories)
                .Where(p => p.PaymentCategoryId == housingDuePaymentCategory.Id &&
                            p.HousingPaymentPlanGroupHousingCategories.Any(p =>
                                p.HousingCategoryId == housing.HousingCategoryId)).ToListAsync();


            //Check housingCategory
            if (housingExistsInHousingPaymentPlanGroup.Any() == false)
            {
                throw new UserFriendlyException("Ödeme kategorisi konut grubu uyumsuz.");
            }

            var toPaymentAccount =
                await _paymentAccountRepository.GetAsync(input.ToPaymentAccountId.GetValueOrDefault());

            var accountBookGuid = SequentialGuidGenerator.Instance.Create();

            var accountBookFiles = new List<AccountBookFile>();
            foreach (var accountBookFileUrl in input.AccountBookFileUrls)
            {
                var newFileUrl = await _blobService.MoveBetweenContainersAsync(accountBookFileUrl,
                    AppConstants.TempContainerName,
                    AppConstants.AccountBookContainerName);

                var entity = AccountBookFile.Create(
                    SequentialGuidGenerator.Instance.Create()
                    , AbpSession.GetTenantId()
                    , newFileUrl
                    , accountBookGuid
                    , AbpSession.UserId.Value
                );
                accountBookFiles.Add(entity);
            }

            var accountBook = await AccountBook.CreateHousingDueAsync(
                _accountBookPolicy
                , accountBookGuid
                , AbpSession.GetTenantId()
                , input.ProcessDateTime
                , housingDuePaymentCategory.Id
                , input.HousingId
                , toPaymentAccount
                , input.Amount
                , input.Description
                , accountBookFiles
                , AbpSession.GetUserId());

            await _accountBookManager.CreateForHousingDueAsync(accountBook, housing, toPaymentAccount,
                organizeBalances);

            return accountBook;
        }

        private async Task<AccountBook> CreateOtherPaymentAsync(CreateAccountBookDto input, bool organizeBalances)
        {
            CheckCreatePermission();
            input.ProcessDateTime = input.ProcessDateTime.Date + new TimeSpan(0, 0, 0);

            PaymentAccount fromPaymentAccount = null;
            PaymentAccount toPaymentAccount = null;

            if (input.FromPaymentAccountId.HasValue)
            {
                fromPaymentAccount = await _paymentAccountRepository.GetAsync(input.FromPaymentAccountId.Value);
            }

            if (input.ToPaymentAccountId.HasValue)
            {
                toPaymentAccount = await _paymentAccountRepository.GetAsync(input.ToPaymentAccountId.Value);
            }

            await _paymentCategoryManager.GetAsync(input.PaymentCategoryId);

            var accountBookGuid = SequentialGuidGenerator.Instance.Create();

            var accountBookFiles = new List<AccountBookFile>();
            foreach (var accountBookFileUrl in input.AccountBookFileUrls)
            {
                var newFileUrl = await _blobService.MoveBetweenContainersAsync(accountBookFileUrl,
                    AppConstants.TempContainerName,
                    AppConstants.AccountBookContainerName);

                var entity = AccountBookFile.Create(
                    SequentialGuidGenerator.Instance.Create()
                    , AbpSession.GetTenantId()
                    , newFileUrl
                    , accountBookGuid
                    , AbpSession.UserId.Value
                );
                accountBookFiles.Add(entity);
            }

            var accountBookType = input.NettingFromHousingDue //Mahsuplaşma var mı kontrolü yapılıyor
                ? AccountBookType.OtherPaymentWithNettingForHousingDue
                : AccountBookType.Other;

            var accountBook = await AccountBook.CreateAsync(
                _accountBookPolicy
                , accountBookGuid
                , AbpSession.GetTenantId()
                , accountBookType
                , input.ProcessDateTime
                , input.PaymentCategoryId
                , null
                , input.NettingFromHousingDue
                , input.HousingIdForNetting
                , input.PaymentCategoryIdForNetting
                , fromPaymentAccount
                , toPaymentAccount
                , input.Amount
                , input.Description
                , input.DocumentDateTime
                , input.DocumentNumber
                , accountBookFiles
                , AbpSession.GetUserId());

            if (input.NettingFromHousingDue && input.HousingIdForNetting.HasValue
            ) //Mahsuplaşma kaydı gerçekleştiriyor
            {
                if (input.FromPaymentAccountId.HasValue && fromPaymentAccount.TenantIsOwner)
                {
                    throw new UserFriendlyException("'Ödeme hesabından' seçeneği, siteye ait bir hesap olamaz.");
                }

                var nettingHousing = await _housingRepository.GetAsync(input.HousingIdForNetting.Value);
                var nettingPaymentCategory =
                    await _paymentCategoryRepository.GetAsync(input.PaymentCategoryIdForNetting.GetValueOrDefault());

                await _accountBookManager.CreateOtherPaymentWithNettingForHousingDueAsync(accountBook,
                    nettingHousing,
                    input.FromPaymentAccountId.HasValue ? fromPaymentAccount : null,
                    input.ToPaymentAccountId.HasValue ? toPaymentAccount : null,
                    nettingPaymentCategory,
                    organizeBalances);
            }
            else
            {
                await _accountBookManager.CreateAsync(accountBook, accountBookType,
                    input.FromPaymentAccountId.HasValue ? fromPaymentAccount : null,
                    input.ToPaymentAccountId.HasValue ? toPaymentAccount : null, null, null, null, organizeBalances);
            }

            return accountBook;
        }

        private async Task<AccountBookDto> DeleteAndCreateAsync(UpdateAccountBookDto input,
            AccountBook existingAccountBook)
        {
            var accountBookFileUrls =
                existingAccountBook.AccountBookFiles.Select(p => p.FileUrl).ToList();

            foreach (var newAccountBookFileUrl in input.NewAccountBookFileUrls)
            {
                accountBookFileUrls.Add(newAccountBookFileUrl);
            }

            foreach (var deletedAccountBookFileUrl in input.DeletedAccountBookFileUrls)
            {
                accountBookFileUrls.Remove(deletedAccountBookFileUrl);
            }

            // İşletme defteri silinip, tekrar oluşturulacak
            //Yeniden oluşturma için gerekli bilgiler toplanıyor
            var accountBookType = existingAccountBook.AccountBookType;

            AccountBook newAccountBook;

            if (accountBookType == AccountBookType.HousingDue)
            {
                var createHousingDueAccountBookDto = new CreateAccountBookDto
                {
                    ProcessDateTime = input.ProcessDateTime,
                    HousingId = existingAccountBook.HousingId.Value,
                    ToPaymentAccountId = input.ToPaymentAccountId.Value,
                    Amount = input.Amount,
                    Description = input.Description,
                    PaymentCategoryId = input.PaymentCategoryId,
                    AccountBookFileUrls = accountBookFileUrls
                };

                newAccountBook = await CreateHousingDueAsync(createHousingDueAccountBookDto, false);
                await _accountBookManager.DeleteAsync(existingAccountBook, false);
            }
            else
            {
                var createOtherPaymentAccountBookDto = new CreateAccountBookDto
                {
                    ProcessDateTime = input.ProcessDateTime,
                    PaymentCategoryId = input.PaymentCategoryId,
                    FromPaymentAccountId =
                        input.FromPaymentAccountId.HasValue ? input.FromPaymentAccountId.Value : null,
                    ToPaymentAccountId = input.ToPaymentAccountId.HasValue ? input.ToPaymentAccountId.Value : null,
                    Amount = input.Amount,
                    Description = input.Description,
                    DocumentDateTime = input.DocumentDateTime,
                    DocumentNumber = input.DocumentNumber,
                    AccountBookFileUrls = accountBookFileUrls,
                    NettingFromHousingDue = existingAccountBook.NettingHousing,
                    HousingIdForNetting = existingAccountBook.HousingIdForNetting
                };

                await _accountBookManager.DeleteAsync(existingAccountBook, false);
                newAccountBook = await CreateOtherPaymentAsync(createOtherPaymentAccountBookDto, false);
            }

            //Organize Balances
            var paymentAccounts = new List<PaymentAccount>();

            if (input.FromPaymentAccountId.HasValue)
            {
                var inputFromPaymentAccount =
                    await _paymentAccountRepository.GetAsync(input.FromPaymentAccountId.GetValueOrDefault());
                paymentAccounts.Add(inputFromPaymentAccount);
            }

            if (input.ToPaymentAccountId.HasValue)
            {
                var inputToPaymentAccount =
                    await _paymentAccountRepository.GetAsync(input.ToPaymentAccountId.GetValueOrDefault());
                paymentAccounts.Add(inputToPaymentAccount);
            }

            if (existingAccountBook.FromPaymentAccountId.HasValue)
            {
                var existingAccountBookFromPaymentAccount =
                    await _paymentAccountRepository.GetAsync(existingAccountBook.FromPaymentAccountId
                        .GetValueOrDefault());
                paymentAccounts.Add(existingAccountBookFromPaymentAccount);
            }

            if (existingAccountBook.ToPaymentAccountId.HasValue)
            {
                var existingAccountBookToPaymentAccount =
                    await _paymentAccountRepository.GetAsync(existingAccountBook.ToPaymentAccountId
                        .GetValueOrDefault());
                paymentAccounts.Add(existingAccountBookToPaymentAccount);
            }

            await _balanceOrganizer.GetOrganizedAccountBooksAsync(
                input.ProcessDateTime < existingAccountBook.ProcessDateTime
                    ? input.ProcessDateTime
                    : existingAccountBook.ProcessDateTime,
                paymentAccounts,
                new List<AccountBook> {newAccountBook},
                null,
                new List<AccountBook> {existingAccountBook}
            );
            _balanceOrganizer.OrganizeAccountBookBalances();
            _balanceOrganizer.OrganizePaymentAccountBalances();

            return ObjectMapper.Map<AccountBookDto>(newAccountBook);
        }

        public override async Task<AccountBookDto> UpdateAsync(UpdateAccountBookDto input)
        {
            try
            {
                CheckUpdatePermission();
                input.ProcessDateTime = input.ProcessDateTime.Date + new TimeSpan(0, 0, 0);

                var existingAccountBook = await _accountBookRepository.GetAsync(input.Id);

                var fromPaymentAccount = existingAccountBook.FromPaymentAccountId.HasValue
                    ? await _paymentAccountRepository.GetAsync(existingAccountBook.FromPaymentAccountId.Value)
                    : null;

                var toPaymentAccount = existingAccountBook.ToPaymentAccountId.HasValue
                    ? await _paymentAccountRepository.GetAsync(existingAccountBook.ToPaymentAccountId.Value)
                    : null;

                if (input.ProcessDateTime != existingAccountBook.ProcessDateTime)
                {
                    //eski tarih ile yeni tarih arasında ödeme hesaplarına ait işlem var mı
                    var oldDate = input.ProcessDateTime < existingAccountBook.ProcessDateTime
                        ? input.ProcessDateTime
                        : existingAccountBook.ProcessDateTime;

                    var newDate = oldDate == input.ProcessDateTime
                        ? existingAccountBook.ProcessDateTime
                        : input.ProcessDateTime;

                    var accountBooksBetweenTwoDates = from p in _accountBookRepository.GetAll()
                        where
                            (
                                (p.FromPaymentAccountId == existingAccountBook.FromPaymentAccountId
                                 || p.ToPaymentAccountId == existingAccountBook.FromPaymentAccountId)
                                ||
                                (p.FromPaymentAccountId == existingAccountBook.ToPaymentAccountId
                                 || p.ToPaymentAccountId == existingAccountBook.ToPaymentAccountId)
                            ) &&
                            p.ProcessDateTime > oldDate && p.ProcessDateTime < newDate
                        select p;

                    if (accountBooksBetweenTwoDates.Any())
                    {
                        return await DeleteAndCreateAsync(input, existingAccountBook);
                    }
                }

                if (input.FromPaymentAccountId != existingAccountBook.FromPaymentAccountId ||
                    input.ToPaymentAccountId != existingAccountBook.ToPaymentAccountId)
                {
                    return await DeleteAndCreateAsync(input, existingAccountBook);
                }

                var paymentCategory =
                    await _paymentCategoryRepository.GetAsync(input.PaymentCategoryId);

                if (input.PaymentCategoryId != existingAccountBook.PaymentCategoryId)
                {
                    var oldPaymentCategory =
                        await _paymentCategoryRepository.GetAsync(existingAccountBook.PaymentCategoryId
                            .GetValueOrDefault());

                    if (oldPaymentCategory.PaymentCategoryType != paymentCategory.PaymentCategoryType ||
                        oldPaymentCategory.IsHousingDue != paymentCategory.IsHousingDue)
                    {
                        throw new UserFriendlyException(
                            "Ödeme kategorisi, aynı tipte bir ödeme kategorisi ile değiştirilebilir. (Örn. gelir vs gelir ya da gider vs gider)");
                    }

                    if (oldPaymentCategory.IsHousingDue)
                    {
                        return await DeleteAndCreateAsync(input, existingAccountBook);
                    }
                }

                var existingHousingPaymentPlan = await _housingPaymetPlanRepository.GetAll()
                    .Where(p => p.AccountBookId == existingAccountBook.Id).SingleOrDefaultAsync();
                if (existingHousingPaymentPlan != null)
                {
                    if (input.ProcessDateTime != existingAccountBook.ProcessDateTime
                        || input.Amount != existingAccountBook.Amount)
                    {
                        await _housingPaymentPlanManager.UpdateAsync(existingHousingPaymentPlan.Id,
                            input.ProcessDateTime, input.Amount, existingHousingPaymentPlan.Description);
                    }
                }

                var existingAccountBookForCheck = AccountBook.ShallowCopy(existingAccountBook);

                var newAccountBookFiles = new List<AccountBookFile>();
                input.NewAccountBookFileUrls.ForEach(async newAccountBookFileUrl =>
                {
                    var accountBookFile = AccountBookFile.Create(SequentialGuidGenerator.Instance.Create(),
                        AbpSession.GetTenantId(),
                        newAccountBookFileUrl, existingAccountBook.Id, AbpSession.UserId.Value);
                    await _accountBookFileRepository.InsertAsync(accountBookFile);
                });

                var deletedAccountBookFiles = new List<AccountBookFile>();
                foreach (var deletedAccountBookFileUrl in input.DeletedAccountBookFileUrls)
                {
                    var deletedAccountBookFile = await _accountBookFileRepository.GetAll()
                        .Where(p => p.FileUrl == deletedAccountBookFileUrl).SingleAsync();

                    await _accountBookFileRepository.DeleteAsync(deletedAccountBookFile);
                    deletedAccountBookFiles.Add(deletedAccountBookFile);
                }

                var accountBook = await AccountBook.UpdateAsync(
                    _accountBookPolicy
                    , existingAccountBook
                    , input.ProcessDateTime
                    , paymentCategory
                    , fromPaymentAccount
                    , toPaymentAccount
                    , input.Amount
                    , input.Description
                    , input.DocumentDateTime
                    , input.DocumentNumber
                    , newAccountBookFiles
                    , deletedAccountBookFiles
                    , AbpSession.GetUserId()
                );
                await _accountBookManager.UpdateAsync(existingAccountBookForCheck, accountBook);

                return ObjectMapper.Map<AccountBookDto>(accountBook);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public override async Task DeleteAsync(EntityDto<Guid> input)
        {
            CheckDeletePermission();
            var accountBook = await _accountBookManager.GetAsync(input.Id);
            await _accountBookManager.DeleteAsync(accountBook, true);

            //ToDo transaction commit olduktan sonra azure'dan silinen ilgili dosyaları sil. 
            //Committen sonra silinmesi gerekeceği için bu katmanda sil.
        }

        public override async Task<AccountBookDto> GetAsync(EntityDto<Guid> input)
        {
            CheckGetPermission();
            var accountBook = await _accountBookRepository.GetAll().Where(p => p.Id == input.Id)
                .Include(p => p.AccountBookFiles)
                .SingleAsync();

            return ObjectMapper.Map<AccountBookDto>(accountBook);
        }

        public async Task<PagedAccountBookResultDto> GetAllListAsync(
            PagedAccountBookResultRequestDto input)
        {
            CheckGetAllPermission();
            var housingIdsFromPersonFilter = await _housingManager.GetHousingsFromPersonIds(input.PersonIds);

            var query = (from accountBook in _accountBookRepository.GetAll()
                    join paymentCategory in _paymentCategoryRepository.GetAll() on accountBook.PaymentCategoryId
                        equals paymentCategory.Id into nullablePaymentCategory
                    from paymentCategory in nullablePaymentCategory.DefaultIfEmpty()
                    join housing in _housingRepository.GetAll() on accountBook.HousingId equals housing.Id into
                        nullableHousing
                    from housing in nullableHousing.DefaultIfEmpty()
                    join block in _blockRepository.GetAll() on housing.BlockId equals block.Id into nullableBlock
                    from block in nullableBlock.DefaultIfEmpty()
                    join fromPaymentAccount in _paymentAccountRepository.GetAll() on accountBook
                        .FromPaymentAccountId equals fromPaymentAccount.Id into nullableFromPaymentAccount
                    from fromPaymentAccount in nullableFromPaymentAccount.DefaultIfEmpty()
                    join toPaymentAccount in _paymentAccountRepository.GetAll() on accountBook
                            .ToPaymentAccountId
                        equals toPaymentAccount.Id into nullableToPaymentAccount
                    from toPaymentAccount in nullableToPaymentAccount.DefaultIfEmpty()
                    join nettingHousing in _housingRepository.GetAll().Include(p => p.Block) on accountBook
                            .HousingIdForNetting
                        equals nettingHousing.Id into
                        nullableNetting
                    from nettingHousing in nullableNetting.DefaultIfEmpty()
                    select new
                    {
                        accountBook,
                        paymentCategory,
                        housing,
                        block,
                        fromPaymentAccount,
                        toPaymentAccount,
                        nettingHousing
                    })
                .WhereIf(input.StartDate.HasValue, p => p.accountBook.ProcessDateTime > input.StartDate.Value)
                .WhereIf(input.EndDate.HasValue, p => p.accountBook.ProcessDateTime < input.EndDate.Value)
                .WhereIf(input.HousingIds.Count > 0,
                    p => input.HousingIds.Contains(p.accountBook.HousingId ?? Guid.Empty))
                .WhereIf(input.PaymentCategoryIds.Count > 0,
                    p => input.PaymentCategoryIds.Contains(p.accountBook.PaymentCategoryId ?? Guid.Empty))
                .WhereIf(housingIdsFromPersonFilter.Count > 0,
                    p => housingIdsFromPersonFilter.Select(s => s.Id)
                        .Contains(p.accountBook.HousingId ?? Guid.Empty))
                .WhereIf(input.FromPaymentAccountIds.Count > 0,
                    p => input.FromPaymentAccountIds.Contains(p.accountBook.FromPaymentAccountId ?? Guid.Empty))
                .WhereIf(input.ToPaymentAccountIds.Count > 0,
                    p => input.ToPaymentAccountIds.Contains(p.accountBook.ToPaymentAccountId ?? Guid.Empty))
                .Select(p => new AccountBookGetAllOutput
                {
                    Id = p.accountBook.Id,
                    AccountBookType = p.accountBook.AccountBookType,
                    CreationTime = p.accountBook.CreationTime,
                    CreatorUserId = p.accountBook.CreatorUserId,
                    LastModificationTime = p.accountBook.LastModificationTime,
                    LastModifierUserId = p.accountBook.LastModifierUserId,
                    ProcessDateTime = p.accountBook.ProcessDateTime,
                    PaymentCategoryName =
                        p.paymentCategory != null
                            ? p.paymentCategory.PaymentCategoryName
                            : p.accountBook.AccountBookType == AccountBookType.TransferForPaymentAccount
                                ? "Ödeme hesabı devir"
                                : string.Empty,
                    HousingName = p.housing != null
                        ? p.block.BlockName + "-" + p.housing.Apartment
                        : string.Empty,
                    Amount = p.accountBook.Amount,
                    FromPaymentAccountName = p.fromPaymentAccount != null
                        ? p.fromPaymentAccount.AccountName
                        : string.Empty,
                    ToPaymentAccountName = p.toPaymentAccount != null
                        ? p.toPaymentAccount.AccountName
                        : string.Empty,
                    FromPaymentAccountBalance = p.accountBook.FromPaymentAccountCurrentBalance,
                    ToPaymentAccountBalance = p.accountBook.ToPaymentAccountCurrentBalance,
                    NettingHousing = p.accountBook.NettingHousing,
                    NettingHousingBlockApartment = p.nettingHousing != null
                        ? p.nettingHousing.Block.BlockName + ' ' + p.nettingHousing.Apartment
                        : string.Empty,
                    SameDayIndex = p.accountBook.SameDayIndex,
                    AccountBookFiles = p.accountBook.AccountBookFiles.Select(p => p.FileUrl).ToList()
                });

            var accountBooks = await query
                .OrderBy(input.Sorting ??
                         $"{nameof(AccountBookDto.ProcessDateTime)} DESC, {nameof(AccountBookDto.SameDayIndex)} DESC")
                .PageBy(input)
                .ToListAsync();

            var lastAccountBookDate =
                await _accountBookRepository.GetAll().OrderByDescending(p => p.ProcessDateTime)
                    .FirstOrDefaultAsync();
            return new PagedAccountBookResultDto(await query.CountAsync(), accountBooks,
                lastAccountBookDate?.ProcessDateTime);
        }

        public async Task<List<LookUpDto>> GetPaymentCategoryLookUpForEditAccountBookAsync(
            Guid accountBookId)
        {
            CheckGetAllPermission();

            var accountBook = await _accountBookRepository.GetAll().Where(p => p.Id == accountBookId).AsNoTracking()
                .SingleAsync();

            var paymentCategory =
                await _paymentCategoryRepository.GetAsync(accountBook.PaymentCategoryId.GetValueOrDefault());

            var paymentCategories = await _paymentCategoryRepository.GetAll()
                .Where(p => (p.IsHousingDue == paymentCategory.IsHousingDue &&
                             p.HousingDueForResidentOrOwner == paymentCategory.HousingDueForResidentOrOwner) &&
                            p.PaymentCategoryType == paymentCategory.PaymentCategoryType)
                .WhereIf(paymentCategory != null, p => p.PaymentCategoryType == paymentCategory.PaymentCategoryType)
                .ToListAsync();

            if (paymentCategory != null && paymentCategory.IsHousingDue)
            {
                var housingCategoryIds = await _paymentCategoryManager.GetHousingCategories(paymentCategory.Id);

                var paymentCategoryIdsByHousingCategoryIds =
                    await _paymentCategoryManager.GetPaymentCategoriesByHousingCategoryIds(housingCategoryIds);

                paymentCategories = paymentCategories
                    .Where(p => paymentCategoryIdsByHousingCategoryIds.Contains(p.Id)).ToList();
            }

            return
                (from l in paymentCategories.OrderBy(p => _localizationSource.GetString(p.PaymentCategoryName))
                    select new LookUpDto(l.Id.ToString(),
                        _localizationSource.GetString(l.PaymentCategoryName)))
                .ToList();
        }
    }
}