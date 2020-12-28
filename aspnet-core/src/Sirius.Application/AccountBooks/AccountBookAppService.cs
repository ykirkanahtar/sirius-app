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
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sirius.AccountBooks.Dto;
using Sirius.AppPaymentAccounts;
using Sirius.EntityFrameworkCore.Repositories;
using Sirius.FileServices;
using Sirius.HousingPaymentPlans;
using Sirius.Housings;
using Sirius.PaymentAccounts;
using Sirius.PaymentCategories;
using Sirius.Shared.Constants;

namespace Sirius.AccountBooks
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
        private readonly IPaymentCategoryManager _paymentCategoryManager;
        private readonly IHousingManager _housingManager;
        private readonly IBlobService _blobService;
        private readonly IAccountBookPolicy _accountBookPolicy;

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
            IAccountBookPolicy accountBookPolicy)
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
                , input.ToPaymentAccountId
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

            var accountBook = await AccountBook.CreateAsync(
                _accountBookPolicy
                , accountBookGuid
                , AbpSession.GetTenantId()
                , input.ProcessDateTime
                , input.PaymentCategoryId
                , null
                , input.FromPaymentAccountId
                , input.ToPaymentAccountId
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
                await _accountBookManager.CreateAsync(accountBook,
                    input.FromPaymentAccountId.HasValue ? fromPaymentAccount : null,
                    input.ToPaymentAccountId.HasValue ? toPaymentAccount : null);
            }

            return ObjectMapper.Map<AccountBookDto>(accountBook);
        }

        public override async Task<AccountBookDto> UpdateAsync(UpdateAccountBookDto input)
        {
            CheckUpdatePermission();
            var existingAccountBook = await _accountBookRepository.GetAsync(input.Id);

            var currentAccountBookFileUrls = existingAccountBook.AccountBookFiles.Select(p => p.FileUrl);
            var inputAccountBookFileUrls = input.AccountBookFiles;

            var newAccountBookFileUrls = inputAccountBookFileUrls
                .Where(accountBookFileUrl => !currentAccountBookFileUrls.Contains(accountBookFileUrl)).ToList();

            var accountBookFiles = new List<AccountBookFile>();
            foreach (var newAccountBookFileUrl in newAccountBookFileUrls)
            {
                var entity = AccountBookFile.Create(
                    SequentialGuidGenerator.Instance.Create()
                    , AbpSession.GetTenantId()
                    , newAccountBookFileUrl
                    , existingAccountBook.Id
                );
                accountBookFiles.Add(entity);
            }

            var existingAccountBookFileUrls = inputAccountBookFileUrls
                .Where(accountBookFileUrl => currentAccountBookFileUrls.Contains(accountBookFileUrl)).ToList();
            foreach (var existingAccountBookFileUrl in existingAccountBookFileUrls)
            {
                var existingAccountBookFile =
                    await _accountBookManager.GetAccountBookFileByUrlAsync(existingAccountBookFileUrl);

                accountBookFiles.Add(existingAccountBookFile);
            }

            // var deletingAccountBookFiles = existingAccountBookFileUrls
            //     .Where(existingAccountBookFileUrl => !inputAccountBookFileUrls.Contains(existingAccountBookFileUrl)).ToList();

            var accountBook = await AccountBook.UpdateAsync(
                _accountBookPolicy
                , existingAccountBook
                , input.Description,
                input.DocumentDateTime,
                input.DocumentNumber
                , accountBookFiles
                , AbpSession.GetUserId()
            );
            await _accountBookManager.UpdateAsync(accountBook);
            return ObjectMapper.Map<AccountBookDto>(accountBook);
        }

        public override async Task DeleteAsync(EntityDto<Guid> input)
        {
            CheckDeletePermission();
            var accountBook = await _accountBookManager.GetAsync(input.Id);
            await _accountBookManager.DeleteAsync(accountBook);
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
                await _accountBookRepository.GetAll().OrderByDescending(p => p.ProcessDateTime).FirstOrDefaultAsync();

            return new PagedAccountBookResultDto(await query.CountAsync(), accountBooks,
                lastAccountBookDate?.ProcessDateTime);
        }
    }
}