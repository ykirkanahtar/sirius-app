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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sirius.AccountBooks;
using Sirius.AccountBooks.Dto;
using Sirius.AppPaymentAccounts;
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

        public PaymentAccountAppService(
            IPaymentAccountManager paymentAccountManager,
            IRepository<PaymentAccount, Guid> paymentAccountRepository,
            IAccountBookManager accountBookManager,
            IPaymentCategoryManager paymentCategoryManager)
            : base(paymentAccountRepository)
        {
            _paymentAccountManager = paymentAccountManager;
            _paymentAccountRepository = paymentAccountRepository;
            _accountBookManager = accountBookManager;
            _paymentCategoryManager = paymentCategoryManager;
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
                , input.CreateTransferForPaymentAccount.Amount
            );
            await _paymentAccountManager.CreateAsync(paymentAccount);

            await CreateAccountBookAsync(input.CreateTransferForPaymentAccount, paymentAccount);

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
                , input.CreateTransferForPaymentAccount.Amount
            );
            await _paymentAccountManager.CreateAsync(paymentAccount);

            await CreateAccountBookAsync(input.CreateTransferForPaymentAccount, paymentAccount);

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
                , input.CreateTransferForPaymentAccount.Amount
            );
            await _paymentAccountManager.CreateAsync(paymentAccount);
            
            await CreateAccountBookAsync(input.CreateTransferForPaymentAccount, paymentAccount);
            
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
                input.PersonId, input.EmployeeId, input.TenantIsOwner, input.IsDefault, input.Iban);
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

            var paymentAccounts = await _paymentAccountRepository.GetAllListAsync();

            return
                (from l in paymentAccounts
                    select new LookUpDto(l.Id.ToString(), l.AccountName)).ToList();
        }

        private async Task CreateAccountBookAsync(CreateTransferForPaymentAccountDto input, PaymentAccount paymentAccount)
        {
            if (input.Amount != 0)
            {
                var paymentCategory = await _paymentCategoryManager.GetTransferForPaymentAccountAsync();

                var accountBook = AccountBook.Create(
                    SequentialGuidGenerator.Instance.Create()
                    , AbpSession.GetTenantId()
                    , input.ProcessDateTime
                    , paymentCategory.Id
                    , null
                    , null
                    , paymentAccount.Id
                    , input.Amount
                    , string.Empty
                    , null
                    , string.Empty
                    , new List<AccountBookFile>()
                    , AbpSession.GetUserId()
                );

                await _accountBookManager.CreateForPaymentAccountTransferAsync(accountBook);
            }
        }
    }
}