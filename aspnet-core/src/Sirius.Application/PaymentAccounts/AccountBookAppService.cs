using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
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
using Sirius.Shared.Constants;

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
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public AccountBookAppService(IAccountBookManager accountBookManager,
            IRepository<AccountBook, Guid> accountBookRepository,
            IPaymentCategoryManager paymentCategoryManager,
            IUnitOfWorkManager unitOfWorkManager,
            IHousingManager housingManager,
            IPaymentAccountManager paymentAccountManager,
            IHousingPaymentPlanManager housingPaymentPlanManager,
            IRepository<PaymentCategory, Guid> paymentCategoryRepository,
            IHousingRepository housingRepository,
            IRepository<Block, Guid> blockRepository,
            IRepository<PaymentAccount, Guid> paymentAccountRepository,
            IBlobService blobService,
            IAccountBookPolicy accountBookPolicy, IRepository<AccountBookFile, Guid> accountBookFileRepository)
            : base(accountBookRepository)
        {
            _accountBookManager = accountBookManager;
            _accountBookRepository = accountBookRepository;
            _paymentCategoryManager = paymentCategoryManager;
            _unitOfWorkManager = unitOfWorkManager;
            _housingManager = housingManager;
            _paymentCategoryRepository = paymentCategoryRepository;
            _housingRepository = housingRepository;
            _blockRepository = blockRepository;
            _paymentAccountRepository = paymentAccountRepository;
            _blobService = blobService;
            _accountBookPolicy = accountBookPolicy;
            _accountBookFileRepository = accountBookFileRepository;
        }

        public override Task<AccountBookDto> CreateAsync(CreateAccountBookDto input)
        {
            throw new NotImplementedException();
        }

        public async Task<AccountBookDto> CreateHousingDueAsync(CreateHousingDueAccountBookDto input)
        {
            CheckCreatePermission();
            var housingDuePaymentCategory = await _paymentCategoryManager.GetRegularHousingDueAsync();
            var housing = await _housingManager.GetAsync(input.HousingId);
            var toPaymentAccount = await _paymentAccountRepository.GetAsync(input.ToPaymentAccountId);

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

        public async Task<AccountBookDto> CreateOtherPaymentAsync(CreateOtherPaymentAccountBookDto input)
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
                );
                accountBookFiles.Add(entity);
            }

            var accountBookType = input.EncachmentFromHousingDue
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
                , AbpSession.GetUserId());

            if (input.EncachmentFromHousingDue && input.HousingIdForEncachment.HasValue)
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
                var createHousingDueAccountBookDto = new CreateHousingDueAccountBookDto
                {
                    ProcessDateTime = input.ProcessDateTime,
                    HousingId = existingAccountBook.HousingId.Value,
                    ToPaymentAccountId = input.ToPaymentAccountId.Value,
                    Amount = input.Amount,
                    Description = input.Description,
                    AccountBookFileUrls = accountBookFileUrls
                };

                var retValue = await CreateHousingDueAsync(createHousingDueAccountBookDto);
                await _accountBookManager.DeleteAsync(existingAccountBook);
                return retValue;
            }
            else
            {
                var createOtherPaymentAccountBookDto = new CreateOtherPaymentAccountBookDto
                {
                    ProcessDateTime = input.ProcessDateTime,
                    PaymentCategoryId = input.PaymentCategoryId,
                    FromPaymentAccountId = input.FromPaymentAccountId.Value,
                    ToPaymentAccountId = input.ToPaymentAccountId.Value,
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

        [UnitOfWork]
        public override async Task<AccountBookDto> UpdateAsync(UpdateAccountBookDto input)
        {
            try
            {
                CheckUpdatePermission();
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

                if (input.PaymentCategoryId != existingAccountBook.PaymentCategoryId)
                {
                    //TODO eğer sabit kategorilerden biri değilse (aidat ödemesi, devir hareketi gibi), direk update çalıştırılabilir, silinip yeniden oluşturulmasına gerek yok
                    return await DeleteAndCreateAsync(input, existingAccountBook);
                }

                var existingAccountBookFiles = existingAccountBook.AccountBookFiles;

                foreach (var newAccountBookFileUrl in input.NewAccountBookFileUrls)
                {
                    var entity = AccountBookFile.Create(
                        SequentialGuidGenerator.Instance.Create()
                        , AbpSession.GetTenantId()
                        , newAccountBookFileUrl
                        , existingAccountBook.Id
                    );
                    existingAccountBookFiles.Add(entity);
                }

                foreach (var deletedAccountBookFileUrl in input.DeletedAccountBookFileUrls)
                {
                    var deletedAccountBookFile = await _accountBookFileRepository.GetAll()
                        .Where(p => p.FileUrl == deletedAccountBookFileUrl).SingleAsync();

                    await _accountBookFileRepository.DeleteAsync(deletedAccountBookFile);
                    existingAccountBookFiles.Remove(deletedAccountBookFile);
                }

                var existingAccountBookForCheck = AccountBook.ShallowCopy(existingAccountBook);

                var accountBook = await AccountBook.UpdateAsync(
                    _accountBookPolicy
                    , existingAccountBook
                    , input.ProcessDateTime
                    , fromPaymentAccount
                    , toPaymentAccount
                    , input.Amount
                    , input.Description
                    , input.DocumentDateTime
                    , input.DocumentNumber
                    , existingAccountBook.AccountBookFiles
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

        public async Task<PagedAccountBookResultDto> GetAllListAsync(
            PagedAccountBookResultRequestDto input)
        {
            try
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
                        select new
                        {
                            accountBook,
                            paymentCategory,
                            subHousing,
                            subBlock,
                            subFromPaymentAccount,
                            subToPaymentAccount
                        })
                    .WhereIf(input.StartDate.HasValue, p => p.accountBook.ProcessDateTime > input.StartDate.Value)
                    .WhereIf(input.EndDate.HasValue, p => p.accountBook.ProcessDateTime < input.EndDate.Value)
                    .WhereIf(input.HousingIds.Count > 0,
                        p => input.HousingIds.Contains(p.accountBook.HousingId ?? Guid.Empty))
                    .WhereIf(input.PaymentCategoryIds.Count > 0,
                        p => input.PaymentCategoryIds.Contains(p.accountBook.PaymentCategoryId))
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
                        AccountBookFiles = p.accountBook.AccountBookFiles.Select(p => p.FileUrl).ToList()
                    });

                var accountBooks = await query
                    .OrderBy(input.Sorting ?? $"{nameof(AccountBookDto.ProcessDateTime)} DESC")
                    .PageBy(input)
                    .ToListAsync();

                var lastAccountBookDate =
                    await _accountBookRepository.GetAll().OrderByDescending(p => p.ProcessDateTime)
                        .FirstOrDefaultAsync();
                return new PagedAccountBookResultDto(await query.CountAsync(), accountBooks,
                    lastAccountBookDate?.ProcessDateTime);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}