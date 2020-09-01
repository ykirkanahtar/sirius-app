using System;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Mvc;
using Sirius.AppPaymentAccounts;
using Sirius.PaymentAccounts.Dto;

namespace Sirius.PaymentAccounts
{
    public class PaymentAccountAppService : AsyncCrudAppService<PaymentAccount, PaymentAccountDto, Guid, PagedPaymentAccountResultRequestDto, CreateCashAccountDto, UpdatePaymentAccountDto>,  IPaymentAccountAppService
    {
        private readonly IPaymentAccountManager _paymentAccountManager;
        private readonly IRepository<PaymentAccount, Guid> _paymentAccountRepository;

        public PaymentAccountAppService(IPaymentAccountManager paymentAccountManager, IRepository<PaymentAccount, Guid> paymentAccountRepository)
            :base(paymentAccountRepository)
        {
            _paymentAccountManager = paymentAccountManager;
            _paymentAccountRepository = paymentAccountRepository;
        }

        public async Task<PaymentAccountDto> CreateAdvanceAccountAsync(CreateBankOrAdvanceAccountDto input)
        {
            var paymentAccount = PaymentAccount.CreateAdvanceAccount(
                    SequentialGuidGenerator.Instance.Create()
                    , AbpSession.GetTenantId()
                    , input.AccountName
                    , input.Description
                    , input.Iban
                    , input.PersonId
                    , input.TenantIsOwner
                    );
            await _paymentAccountManager.CreateAsync(paymentAccount);
            return ObjectMapper.Map<PaymentAccountDto>(paymentAccount);
        }

        public async Task<PaymentAccountDto> CreateBankAccountAsync(CreateBankOrAdvanceAccountDto input)
        {
            var paymentAccount = PaymentAccount.CreateBankAccount(
                    SequentialGuidGenerator.Instance.Create()
                    , AbpSession.GetTenantId()
                    , input.AccountName
                    , input.Description
                    , input.Iban
                    , input.PersonId
                    , input.TenantIsOwner
                    );
            await _paymentAccountManager.CreateAsync(paymentAccount);
            return ObjectMapper.Map<PaymentAccountDto>(paymentAccount);
        }

        public async Task<PaymentAccountDto> CreateCashAccountAsync(CreateCashAccountDto input)
        {
            var paymentAccount = PaymentAccount.CreateCashAccount(
                    SequentialGuidGenerator.Instance.Create()
                    , AbpSession.GetTenantId()
                    , input.AccountName
                    , input.Description
                    , input.PersonId
                    , input.TenantIsOwner
                    );
            await _paymentAccountManager.CreateAsync(paymentAccount);
            return ObjectMapper.Map<PaymentAccountDto>(paymentAccount);
        }

        [NonAction]
        public override Task<PaymentAccountDto> CreateAsync(CreateCashAccountDto input)
        {
            throw  new NotImplementedException();
        }

        public override async Task<PaymentAccountDto> UpdateAsync(UpdatePaymentAccountDto input)
        {
            var existingPaymentAccount = await _paymentAccountRepository.GetAsync(input.Id);
            var paymentAccount = PaymentAccount.Update(existingPaymentAccount, input.AccountName, input.Description,
                input.PersonId, input.TenantIsOwner, input.Iban);
            await _paymentAccountManager.UpdateAsync(paymentAccount);
            return ObjectMapper.Map<PaymentAccountDto>(paymentAccount);
        }
    }
}
