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
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sirius.AccountBooks.Dto;
using Sirius.EntityFrameworkCore.Repositories;
using Sirius.FileServices;
using Sirius.HousingPaymentPlans;
using Sirius.Housings;
using Sirius.Inventories;
using Sirius.Inventories.Dto;
using Sirius.PaymentAccounts.Dto;
using Sirius.PaymentCategories;
using Sirius.Periods;
using Sirius.Shared.Constants;
using Sirius.Shared.Dtos;
using Sirius.Shared.Helper;

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
        private readonly IPeriodManager _periodManager;
        private readonly IInventoryAppService _inventoryAppService;
        private readonly IRepository<Inventory, Guid> _inventoryRepository;

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
            IBalanceOrganizer balanceOrganizer, IPeriodManager periodManager, IInventoryAppService inventoryAppService,
            IRepository<Inventory, Guid> inventoryRepository)
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
            _periodManager = periodManager;
            _inventoryAppService = inventoryAppService;
            _inventoryRepository = inventoryRepository;
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

            var activePeriod = await _periodManager.GetActivePeriod();

            var accountBook = await AccountBook.CreateHousingDueAsync(
                _accountBookPolicy
                , accountBookGuid
                , AbpSession.GetTenantId()
                , activePeriod.Id
                , input.ProcessDateString.StringToDateTime()
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

            var activePeriod = await _periodManager.GetActivePeriod();

            var accountBook = await AccountBook.CreateAsync(
                _accountBookPolicy
                , accountBookGuid
                , AbpSession.GetTenantId()
                , activePeriod.Id
                , accountBookType
                , input.ProcessDateString.StringToDateTime()
                , input.PaymentCategoryId
                , null
                , input.NettingFromHousingDue
                , input.HousingIdForNetting
                , input.PaymentCategoryIdForNetting
                , fromPaymentAccount
                , toPaymentAccount
                , input.Amount
                , input.Description
                , input.DocumentDateTimeString.StringToNullableDateTime()
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
                await _accountBookManager.CreateAsync(accountBook,
                    input.FromPaymentAccountId.HasValue ? fromPaymentAccount : null,
                    input.ToPaymentAccountId.HasValue ? toPaymentAccount : null, null, null, null, organizeBalances);
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            if (input.Inventories.Any())
            {
                foreach (var createInventoryDto in input.Inventories)
                {
                    createInventoryDto.AccountBookId = accountBook.Id;
                    await _inventoryAppService.CreateAsync(createInventoryDto);
                }
            }

            return accountBook;
        }

        private async Task<AccountBookDto> DeleteAndCreateAsync(UpdateAccountBookDto input,
            AccountBook existingAccountBook)
        {
            var processDateTime = input.ProcessDateString.StringToDateTime();

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
                    ProcessDateString = input.ProcessDateString,
                    HousingId = existingAccountBook.HousingId.Value,
                    ToPaymentAccountId = input.ToPaymentAccountId.Value,
                    Amount = input.Amount,
                    Description = input.Description,
                    PaymentCategoryId = input.PaymentCategoryId,
                    AccountBookFileUrls = accountBookFileUrls,
                };

                newAccountBook = await CreateHousingDueAsync(createHousingDueAccountBookDto, false);
                await _accountBookManager.DeleteAsync(existingAccountBook, false);
            }
            else
            {
                var createOtherPaymentAccountBookDto = new CreateAccountBookDto
                {
                    ProcessDateString = input.ProcessDateString,
                    PaymentCategoryId = input.PaymentCategoryId,
                    FromPaymentAccountId =
                        input.FromPaymentAccountId.HasValue ? input.FromPaymentAccountId.Value : null,
                    ToPaymentAccountId = input.ToPaymentAccountId.HasValue ? input.ToPaymentAccountId.Value : null,
                    Amount = input.Amount,
                    Description = input.Description,
                    DocumentDateTimeString = input.DocumentDateTimeString,
                    DocumentNumber = input.DocumentNumber,
                    AccountBookFileUrls = accountBookFileUrls,
                    NettingFromHousingDue = existingAccountBook.NettingHousing,
                    HousingIdForNetting = existingAccountBook.HousingIdForNetting,
                    Inventories = input.Inventories
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
                processDateTime < existingAccountBook.ProcessDateTime
                    ? processDateTime
                    : existingAccountBook.ProcessDateTime,
                processDateTime < existingAccountBook.ProcessDateTime
                    ? int.MaxValue
                    : existingAccountBook.SameDayIndex,
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
            CheckUpdatePermission();
            var processDateTime = input.ProcessDateString.StringToDateTime();

            var existingAccountBook = await _accountBookRepository.GetAsync(input.Id);

            var fromPaymentAccount = existingAccountBook.FromPaymentAccountId.HasValue
                ? await _paymentAccountRepository.GetAsync(existingAccountBook.FromPaymentAccountId.Value)
                : null;

            var toPaymentAccount = existingAccountBook.ToPaymentAccountId.HasValue
                ? await _paymentAccountRepository.GetAsync(existingAccountBook.ToPaymentAccountId.Value)
                : null;

            if (processDateTime != existingAccountBook.ProcessDateTime)
            {
                //eski tarih ile yeni tarih arasında ödeme hesaplarına ait işlem var mı
                var oldDate = processDateTime < existingAccountBook.ProcessDateTime
                    ? processDateTime
                    : existingAccountBook.ProcessDateTime;

                var newDate = oldDate == processDateTime
                    ? existingAccountBook.ProcessDateTime
                    : processDateTime;

                var accountBooksBetweenTwoDates = from p in _accountBookRepository.GetAll()
                    where
                        (
                            (p.FromPaymentAccountId == existingAccountBook.FromPaymentAccountId
                             || p.ToPaymentAccountId == existingAccountBook.FromPaymentAccountId)
                            ||
                            (p.FromPaymentAccountId == existingAccountBook.ToPaymentAccountId
                             || p.ToPaymentAccountId == existingAccountBook.ToPaymentAccountId)
                        ) &&
                        (p.ProcessDateTime >= oldDate && p.ProcessDateTime <= newDate)
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
                if (processDateTime != existingAccountBook.ProcessDateTime
                    || input.Amount != existingAccountBook.Amount)
                {
                    await _housingPaymentPlanManager.UpdateAsync(existingHousingPaymentPlan.Id, processDateTime,
                        input.Amount, existingHousingPaymentPlan.Description);
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

            var previousInventories = await _inventoryRepository.GetAll()
                .Where(p => p.AccountBookId == existingAccountBook.Id).ToListAsync();

            var accountBook = await AccountBook.UpdateAsync(
                _accountBookPolicy
                , existingAccountBook
                , processDateTime
                , paymentCategory
                , fromPaymentAccount
                , toPaymentAccount
                , input.Amount
                , input.Description
                , input.DocumentDateTimeString.StringToNullableDateTime()
                , input.DocumentNumber
                , newAccountBookFiles
                , deletedAccountBookFiles
                , AbpSession.GetUserId()
            );
            await _accountBookManager.UpdateAsync(existingAccountBookForCheck, accountBook);

            await CurrentUnitOfWork.SaveChangesAsync();

            if (input.Inventories.Any() || previousInventories.Any())
            {
                //new or updated inventories
                foreach (var createInventoryDto in input.Inventories)
                {
                    var existingInventory = previousInventories.SingleOrDefault(p => p.InventoryTypeId ==
                        createInventoryDto.InventoryTypeId &&
                        p.SerialNumber ==
                        createInventoryDto.SerialNumber);

                    if (existingInventory != null)
                    {
                        if (existingInventory.Quantity != createInventoryDto.Quantity ||
                            existingInventory.Description != createInventoryDto.Description)
                        {
                            var updateInventoryDto = new UpdateInventoryDto
                            {
                                InventoryTypeId = existingInventory.InventoryTypeId,
                                Id = existingInventory.Id,
                                SerialNumber = existingInventory.SerialNumber,
                                AccountBookId = existingInventory.AccountBookId,
                                Quantity = createInventoryDto.Quantity,
                                Description = createInventoryDto.Description
                            };
                            await _inventoryAppService.UpdateAsync(updateInventoryDto);
                        }
                    }
                    else
                    {
                        createInventoryDto.AccountBookId = accountBook.Id;
                        await _inventoryAppService.CreateAsync(createInventoryDto);
                    }
                }

                //deleted inventories
                foreach (var previousInventory in previousInventories)
                {
                    var missingInventories = input.Inventories.Where(p => p.InventoryTypeId ==
                                                                          previousInventory.InventoryTypeId &&
                                                                          p.SerialNumber ==
                                                                          previousInventory.SerialNumber).ToList();

                    if (missingInventories.Any() == false)
                    {
                        await _inventoryAppService.DeleteByIdAsync(previousInventory.Id);
                    }
                }
            }

            return ObjectMapper.Map<AccountBookDto>(accountBook);
        }

        public override async Task DeleteAsync(EntityDto<Guid> input)
        {
            CheckDeletePermission();
            var accountBook = await _accountBookManager.GetAsync(input.Id);
            await _accountBookManager.DeleteAsync(accountBook, true);

            await CurrentUnitOfWork.SaveChangesAsync();

            var deletedInventories =
                await _inventoryRepository.GetAll().Where(p => p.AccountBookId == input.Id).ToListAsync();

            foreach (var deletedInventory in deletedInventories)
            {
                await _inventoryAppService.DeleteByIdAsync(deletedInventory.Id);
            }

            //ToDo transaction commit olduktan sonra azure'dan silinen ilgili dosyaları sil. 
            //Committen sonra silinmesi gerekeceği için bu katmanda sil.
        }

        public override async Task<AccountBookDto> GetAsync(EntityDto<Guid> input)
        {
            CheckGetPermission();
            var accountBook = await _accountBookRepository.GetAll().Where(p => p.Id == input.Id)
                .Include(p => p.AccountBookFiles)
                .Include(p => p.Inventories)
                .ThenInclude(p => p.InventoryType)
                .SingleAsync();

            return ObjectMapper.Map<AccountBookDto>(accountBook);
        }

        private async Task<IQueryable<AccountBookGetAllOutput>> FilterQueryAsync(IAccountBookGetAllFilter filter)
        {
            var housingIdsFromPersonFilter = await _housingManager.GetHousingsFromPersonIds(filter.PersonIds);

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
                .WhereIf(filter.StartDate.HasValue, p => p.accountBook.ProcessDateTime > filter.StartDate.Value)
                .WhereIf(filter.EndDate.HasValue, p => p.accountBook.ProcessDateTime < filter.EndDate.Value)
                .WhereIf(filter.HousingIds.Count > 0,
                    p => filter.HousingIds.Contains(p.accountBook.HousingId ?? Guid.Empty))
                .WhereIf(filter.PaymentCategoryIds.Count > 0,
                    p => filter.PaymentCategoryIds.Contains(p.accountBook.PaymentCategoryId ?? Guid.Empty))
                .WhereIf(housingIdsFromPersonFilter.Count > 0,
                    p => housingIdsFromPersonFilter.Select(s => s.Id)
                        .Contains(p.accountBook.HousingId ?? Guid.Empty))
                .WhereIf(filter.FromPaymentAccountIds.Count > 0,
                    p => filter.FromPaymentAccountIds.Contains(p.accountBook.FromPaymentAccountId ?? Guid.Empty))
                .WhereIf(filter.ToPaymentAccountIds.Count > 0,
                    p => filter.ToPaymentAccountIds.Contains(p.accountBook.ToPaymentAccountId ?? Guid.Empty))
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
                            : GetAccountTypeNameForGetAll(p.accountBook.AccountBookType),
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

            return query.OrderBy(filter.Sorting ??
                                 $"{nameof(AccountBookDto.ProcessDateTime)} DESC, {nameof(AccountBookDto.SameDayIndex)} DESC");
        }

        private static string GetAccountTypeNameForGetAll(AccountBookType accountBookType)
        {
            return accountBookType switch
            {
                AccountBookType.FirstTransferForPaymentAccount => "Ödeme Hesabı İlk Devir",
                AccountBookType.TransferForPaymentAccountToNextPeriod => "Sonraki döneme devir",
                AccountBookType.TransferForPaymentAccountFromPreviousPeriod => "Önceki dönemden devir",
                _ => string.Empty
            };
        }

        public async Task<PagedAccountBookResultDto> GetAllListAsync(
            PagedAccountBookResultRequestDto input)
        {
            CheckGetAllPermission();

            var query = await FilterQueryAsync(input);

            var accountBooks = await query
                .PageBy(input)
                .ToListAsync();

            var lastAccountBookDate =
                await _accountBookRepository.GetAll().OrderByDescending(p => p.ProcessDateTime)
                    .FirstOrDefaultAsync();
            return new PagedAccountBookResultDto(await query.CountAsync(), accountBooks,
                lastAccountBookDate?.ProcessDateTime);
        }

        public async Task<List<AccountBookGetAllExportOutput>> GetAllListForExportAsync(
            AccountBookGetAllFilter input)
        {
            CheckGetAllPermission();

            var query = await FilterQueryAsync(input);

            var accountBookGetAll = await query
                .ToListAsync();
            return ObjectMapper.Map<List<AccountBookGetAllExportOutput>>(accountBookGetAll);
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