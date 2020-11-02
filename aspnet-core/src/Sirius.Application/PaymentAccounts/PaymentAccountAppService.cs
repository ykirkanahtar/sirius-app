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
using Sirius.AppPaymentAccounts;
using Sirius.PaymentAccounts.Dto;
using Sirius.Shared.Dtos;

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

        public async Task<List<LookUpDto>> GetPaymentAccountLookUpAsync()
        {
            var paymentAccounts = await _paymentAccountRepository.GetAllListAsync();         
                                                                               
            return                                                             
                (from l in paymentAccounts                                            
                    select new LookUpDto(l.Id.ToString(), l.AccountName)).ToList();  
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
        
        public override async Task<PagedResultDto<PaymentAccountDto>> GetAllAsync(PagedPaymentAccountResultRequestDto input)
        {
            var query = _paymentAccountRepository.GetAll();

            var paymentAccounts = await query.OrderBy(input.Sorting ?? $"{nameof(PaymentAccountDto.AccountName)}")
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<PaymentAccountDto>(query.Count(),
                ObjectMapper.Map<List<PaymentAccountDto>>(paymentAccounts));
        }
    }
}
