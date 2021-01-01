using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sirius.AccountBooks;
using Sirius.AccountBooks.Dto;
using Sirius.AppPaymentAccounts;
using Sirius.EntityFrameworkCore;
using Sirius.PaymentAccounts.Dto;
using Sirius.PaymentCategories;
using Sirius.Shared.Dtos;

namespace Sirius.PaymentAccounts
{
    public class PaymentAccountAppService :
        AsyncCrudAppService<PaymentAccount, PaymentAccountDto, Guid, PagedPaymentAccountResultRequestDto,
            CreateCashAccountDto, UpdatePaymentAccountDto>, IPaymentAccountAppService
    {
        private readonly IPaymentAccountManager _paymentAccountManager;
        private readonly IRepository<PaymentAccount, Guid> _paymentAccountRepository;
        private readonly IAccountBookManager _accountBookManager;
        private readonly IPaymentCategoryManager _paymentCategoryManager;
        private readonly IAccountBookPolicy _accountBookPolicy;
        private readonly IDbContextProvider<SiriusDbContext> _dbContextProvider;

        public PaymentAccountAppService(
            IPaymentAccountManager paymentAccountManager,
            IRepository<PaymentAccount, Guid> paymentAccountRepository,
            IAccountBookManager accountBookManager,
            IPaymentCategoryManager paymentCategoryManager,
            IAccountBookPolicy accountBookPolicy, IDbContextProvider<SiriusDbContext> dbContextProvider)
            : base(paymentAccountRepository)
        {
            _paymentAccountManager = paymentAccountManager;
            _paymentAccountRepository = paymentAccountRepository;
            _accountBookManager = accountBookManager;
            _paymentCategoryManager = paymentCategoryManager;
            _accountBookPolicy = accountBookPolicy;
            _dbContextProvider = dbContextProvider;
            _accountBookManager = accountBookManager;
        }

        public async Task<PaymentAccountDto> CreateAdvanceAccountAsync(CreateBankOrAdvanceAccountDto input)
        {
            CheckCreatePermission();

            var newPaymentAccountId = SequentialGuidGenerator.Instance.Create();
            var paymentAccount = PaymentAccount.CreateAdvanceAccount(
                newPaymentAccountId
                , AbpSession.GetTenantId()
                , input.AccountName
                , input.Description
                , input.Iban
                , input.PersonId
                , input.EmployeeId
                , input.TenantIsOwner
                , input.IsDefault
                , input.TransferAmount
                , input.FirstTransferDateTime
            );
            await _paymentAccountManager.CreateAsync(paymentAccount);

            await CreateAccountBookAsync(input.TransferAmount, input.FirstTransferDateTime, paymentAccount);

            return ObjectMapper.Map<PaymentAccountDto>(paymentAccount);
        }

        public async Task<PaymentAccountDto> CreateBankAccountAsync(CreateBankOrAdvanceAccountDto input)
        {
            CheckUpdatePermission();

            var newPaymentAccountId = SequentialGuidGenerator.Instance.Create();
            var paymentAccount = PaymentAccount.CreateBankAccount(
                newPaymentAccountId
                , AbpSession.GetTenantId()
                , input.AccountName
                , input.Description
                , input.Iban
                , input.PersonId
                , input.EmployeeId
                , input.TenantIsOwner
                , input.IsDefault
                , input.TransferAmount
                , input.FirstTransferDateTime
            );
            await _paymentAccountManager.CreateAsync(paymentAccount);

            await CreateAccountBookAsync(input.TransferAmount, input.FirstTransferDateTime, paymentAccount);

            return ObjectMapper.Map<PaymentAccountDto>(paymentAccount);
        }

        public async Task<PaymentAccountDto> CreateCashAccountAsync(CreateCashAccountDto input)
        {
            CheckUpdatePermission();

            var newPaymentAccountId = SequentialGuidGenerator.Instance.Create();
            var paymentAccount = PaymentAccount.CreateCashAccount(
                newPaymentAccountId
                , AbpSession.GetTenantId()
                , input.AccountName
                , input.Description
                , input.PersonId
                , input.EmployeeId
                , input.TenantIsOwner
                , input.IsDefault
                , input.TransferAmount
                , input.FirstTransferDateTime
            );
            await _paymentAccountManager.CreateAsync(paymentAccount);

            await CreateAccountBookAsync(input.TransferAmount, input.FirstTransferDateTime, paymentAccount);

            return ObjectMapper.Map<PaymentAccountDto>(paymentAccount);
        }

        [NonAction]
        public override Task<PaymentAccountDto> CreateAsync(CreateCashAccountDto input)
        {
            throw new NotImplementedException();
        }

        public override async Task<PaymentAccountDto> UpdateAsync(UpdatePaymentAccountDto input)
        {
            CheckUpdatePermission();
            var existingPaymentAccount = await _paymentAccountRepository.GetAsync(input.Id);
            var paymentAccount = PaymentAccount.Update(existingPaymentAccount, input.AccountName, input.Description,
                input.PersonId, input.EmployeeId, input.TenantIsOwner, input.IsDefault, existingPaymentAccount.Balance,
                input.Iban);
            await _paymentAccountManager.UpdateAsync(paymentAccount);
            return ObjectMapper.Map<PaymentAccountDto>(paymentAccount);
        }

        public override async Task DeleteAsync(EntityDto<Guid> input)
        {
            CheckDeletePermission();
            var paymentAccount = await _paymentAccountManager.GetAsync(input.Id);
            await _paymentAccountManager.DeleteAsync(paymentAccount);
        }

        public override async Task<PagedResultDto<PaymentAccountDto>> GetAllAsync(
            PagedPaymentAccountResultRequestDto input)
        {
            CheckGetAllPermission();

            var query = _paymentAccountRepository.GetAll();

            var paymentAccounts = await query.OrderBy(input.Sorting ?? $"{nameof(PaymentAccountDto.AccountName)}")
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<PaymentAccountDto>(query.Count(),
                ObjectMapper.Map<List<PaymentAccountDto>>(paymentAccounts));
        }

        public async Task<List<LookUpDto>> GetPaymentAccountLookUpAsync()
        {
            CheckGetAllPermission();

            var paymentAccounts = await _paymentAccountRepository.GetAll().OrderBy(p => p.AccountName).ToListAsync();

            return
                (from l in paymentAccounts
                    select new LookUpDto(l.Id.ToString(), l.AccountName)).ToList();
        }

        public async Task<PaymentAccountDto> GetDefaultPaymentAccountAsync()
        {
            CheckGetAllPermission();

            var defaultPaymentAccount =
                await _paymentAccountRepository.GetAll().Where(p => p.IsDefault).SingleOrDefaultAsync();
            return ObjectMapper.Map<PaymentAccountDto>(defaultPaymentAccount);
        }

        private async Task CreateAccountBookAsync(decimal? transferAmount, DateTime? transferProcessDateTime,
            PaymentAccount paymentAccount)
        {
            if (transferAmount.HasValue && transferProcessDateTime.HasValue)
            {
                var paymentCategory = await _paymentCategoryManager.GetTransferForPaymentAccountAsync();

                var accountBook = await AccountBook.CreateForPaymentAccountTransferAsync(
                    _accountBookPolicy
                    , SequentialGuidGenerator.Instance.Create()
                    , AbpSession.GetTenantId()
                    , transferProcessDateTime.Value
                    , paymentCategory.Id
                    , paymentAccount.Id
                    , transferAmount.Value
                    , string.Empty
                    , new List<AccountBookFile>()
                    , AbpSession.GetUserId()
                );

                await _accountBookManager.CreateForPaymentAccountTransferAsync(accountBook,
                    _dbContextProvider.GetDbContext());
            }
        }
    }
}