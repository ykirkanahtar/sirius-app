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
        private readonly IHousingPaymentPlanManager _housingPaymentPlanManager;
        private readonly ILocalizationSource _localizationSource;

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
            ILocalizationManager localizationManager)
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
            _localizationSource = localizationManager.GetSource(AppConstants.LocalizationSourceName);
        }

        public override async Task<AccountBookDto> CreateAsync(CreateAccountBookDto input)
        {
            CheckCreatePermission();
            if (input.IsHousingDue)
            {
                return await CreateHousingDueAsync(input);
            }

            return await CreateOtherPaymentAsync(input);
        }

        private async Task<AccountBookDto> CreateHousingDueAsync(CreateAccountBookDto input)
        {
            CheckCreatePermission();
            input.ProcessDateTime = input.ProcessDateTime.Date + new TimeSpan(0, 0, 0);

            // var housingDuePaymentCategory = await _paymentCategoryManager.GetRegularHousingDueAsync();
            var housingDuePaymentCategory = await _paymentCategoryManager.GetAsync(input.PaymentCategoryId);

            var housing = await _housingManager.GetAsync(input.HousingId);
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

            await _accountBookManager.CreateForHousingDueAsync(accountBook, housing, toPaymentAccount);

            return ObjectMapper.Map<AccountBookDto>(accountBook);
        }

        private async Task<AccountBookDto> CreateOtherPaymentAsync(CreateAccountBookDto input)
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

            var accountBookType = input.EncachmentFromHousingDue //Mahsuplaşma var mı kontrolü yapılıyor
                ? AccountBookType.OtherPaymentWithEncachmentForHousingDue
                : AccountBookType.Other;

            var accountBook = await AccountBook.CreateAsync(
                _accountBookPolicy
                , accountBookGuid
                , AbpSession.GetTenantId()
                , accountBookType
                , input.ProcessDateTime
                , input.PaymentCategoryId
                , null
                , input.EncachmentFromHousingDue
                , input.HousingIdForEncachment
                , fromPaymentAccount
                , toPaymentAccount
                , input.Amount
                , input.Description
                , input.DocumentDateTime
                , input.DocumentNumber
                , accountBookFiles
                , AbpSession.GetUserId()
                , false);

            if (input.EncachmentFromHousingDue && input.HousingIdForEncachment.HasValue
            ) //Mahsuplaşma kaydı gerçekleştiriyor
            {
                if (input.FromPaymentAccountId.HasValue && fromPaymentAccount.TenantIsOwner)
                {
                    throw new UserFriendlyException("'Ödeme hesabından' seçeneği, siteye ait bir hesap olamaz.");
                }

                var encashmentHousing = await _housingRepository.GetAsync(input.HousingIdForEncachment.Value);

                await _accountBookManager.CreateOtherPaymentWithEncachmentForHousingDueAsync(accountBook,
                    encashmentHousing,
                    input.FromPaymentAccountId.HasValue ? fromPaymentAccount : null,
                    input.ToPaymentAccountId.HasValue ? toPaymentAccount : null);
            }
            else
            {
                await _accountBookManager.CreateAsync(accountBook, accountBookType,
                    input.FromPaymentAccountId.HasValue ? fromPaymentAccount : null,
                    input.ToPaymentAccountId.HasValue ? toPaymentAccount : null, null);
            }

            return ObjectMapper.Map<AccountBookDto>(accountBook);
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

                var retValue = await CreateHousingDueAsync(createHousingDueAccountBookDto);
                await _accountBookManager.DeleteAsync(existingAccountBook);
                return retValue;
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
                    EncachmentFromHousingDue = existingAccountBook.EncashmentHousing,
                    HousingIdForEncachment = existingAccountBook.HousingIdForEncachment
                };

                await _accountBookManager.DeleteAsync(existingAccountBook);
                await UnitOfWorkManager.Current.SaveChangesAsync();
                return await CreateOtherPaymentAsync(createOtherPaymentAccountBookDto);
            }
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
            await _accountBookManager.DeleteAsync(accountBook);

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
                        equals paymentCategory.Id
                    join housing in _housingRepository.GetAll() on accountBook.HousingId equals housing.Id into
                        housing
                    from subHousing in housing.DefaultIfEmpty()
                    join block in _blockRepository.GetAll() on subHousing.BlockId equals block.Id into block
                    from subBlock in block.DefaultIfEmpty()
                    join fromPaymentAccount in _paymentAccountRepository.GetAll() on accountBook
                        .FromPaymentAccountId equals fromPaymentAccount.Id into fromPaymentAccount
                    from subFromPaymentAccount in fromPaymentAccount.DefaultIfEmpty()
                    join toPaymentAccount in _paymentAccountRepository.GetAll() on accountBook
                            .ToPaymentAccountId
                        equals toPaymentAccount.Id into toPaymentAccount
                    from subToPaymentAccount in toPaymentAccount.DefaultIfEmpty()
                    join encashmentHousing in _housingRepository.GetAll().Include(p => p.Block) on accountBook
                            .HousingIdForEncachment
                        equals encashmentHousing.Id into
                        encashmentHousing
                    from subEncashmentHousing in encashmentHousing.DefaultIfEmpty()
                    select new
                    {
                        accountBook,
                        paymentCategory,
                        subHousing,
                        subBlock,
                        subFromPaymentAccount,
                        subToPaymentAccount,
                        subEncashmentHousing
                    })
                .WhereIf(input.StartDate.HasValue, p => p.accountBook.ProcessDateTime > input.StartDate.Value)
                .WhereIf(input.EndDate.HasValue, p => p.accountBook.ProcessDateTime < input.EndDate.Value)
                .WhereIf(input.HousingIds.Count > 0,
                    p => input.HousingIds.Contains(p.accountBook.HousingId ?? Guid.Empty))
                .WhereIf(input.PaymentCategoryIds.Count > 0,
                    p => input.PaymentCategoryIds.Contains(p.accountBook.PaymentCategoryId.GetValueOrDefault()))
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
                    CreationTime = p.accountBook.CreationTime,
                    CreatorUserId = p.accountBook.CreatorUserId,
                    LastModificationTime = p.accountBook.LastModificationTime,
                    LastModifierUserId = p.accountBook.LastModifierUserId,
                    ProcessDateTime = p.accountBook.ProcessDateTime,
                    PaymentCategoryName = p.paymentCategory.PaymentCategoryName,
                    HousingName = p.subHousing != null
                        ? p.subBlock.BlockName + "-" + p.subHousing.Apartment
                        : string.Empty,
                    Amount = p.accountBook.Amount,
                    FromPaymentAccountName = p.subFromPaymentAccount != null
                        ? p.subFromPaymentAccount.AccountName
                        : string.Empty,
                    ToPaymentAccountName = p.subToPaymentAccount != null
                        ? p.subToPaymentAccount.AccountName
                        : string.Empty,
                    FromPaymentAccountBalance = p.accountBook.FromPaymentAccountCurrentBalance,
                    ToPaymentAccountBalance = p.accountBook.ToPaymentAccountCurrentBalance,
                    EncashmentHousing = p.accountBook.EncashmentHousing,
                    EncashmentHousingBlockApartment = p.subEncashmentHousing != null
                        ? p.subEncashmentHousing.Block.BlockName + ' ' + p.subEncashmentHousing.Apartment
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

            // if (!paymentCategory.EditInAccountBook
            // ) //Eğer düzenlemeye izin verilmeyen bir ödeme türü ise, sadece o döndürülüyor
            // {
            //     return new List<PaymentCategoryLookUpDto>
            //     {
            //         new(paymentCategory.Id.ToString(),
            //             _localizationSource.GetString(paymentCategory.PaymentCategoryName),
            //             paymentCategory.EditInAccountBook)
            //     };
            // }

            var paymentCategories = await _paymentCategoryRepository.GetAll()
                .Where(p => p.IsHousingDue == paymentCategory.IsHousingDue &&
                            p.PaymentCategoryType == paymentCategory.PaymentCategoryType)
                .WhereIf(paymentCategory != null, p => p.PaymentCategoryType == paymentCategory.PaymentCategoryType)
                .ToListAsync();

            return
                (from l in paymentCategories.OrderBy(p => _localizationSource.GetString(p.PaymentCategoryName))
                    select new LookUpDto(l.Id.ToString(),
                        _localizationSource.GetString(l.PaymentCategoryName)))
                .ToList();
        }
    }
}