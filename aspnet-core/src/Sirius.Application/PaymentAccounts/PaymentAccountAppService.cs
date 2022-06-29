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
using Sirius.EntityFrameworkCore;
using Sirius.PaymentAccounts.Dto;
using Sirius.PaymentCategories;
using Sirius.Periods;
using Sirius.Shared.Dtos;
using Sirius.Shared.Helper;

namespace Sirius.PaymentAccounts
{
    public class PaymentAccountAppService :
        AsyncCrudAppService<PaymentAccount, PaymentAccountDto, Guid, PagedPaymentAccountResultRequestDto,
            CreateCashAccountDto, UpdatePaymentAccountDto>, IPaymentAccountAppService
    {
        private readonly IPaymentAccountManager _paymentAccountManager;
        private readonly IRepository<AccountBook, Guid> _accountBookRepository;
        private readonly IRepository<PaymentAccount, Guid> _paymentAccountRepository;
        private readonly IAccountBookManager _accountBookManager;
        private readonly IAccountBookPolicy _accountBookPolicy;
        private readonly IPeriodManager _periodManager;
        private readonly IBalanceOrganizer _balanceOrganizer;

        public PaymentAccountAppService(
            IPaymentAccountManager paymentAccountManager,
            IRepository<PaymentAccount, Guid> paymentAccountRepository,
            IAccountBookManager accountBookManager,
            IAccountBookPolicy accountBookPolicy,
            IPeriodManager periodManager,
            IBalanceOrganizer balanceOrganizer,
            IRepository<AccountBook, Guid> accountBookRepository)
            : base(paymentAccountRepository)
        {
            _paymentAccountManager = paymentAccountManager;
            _paymentAccountRepository = paymentAccountRepository;
            _accountBookManager = accountBookManager;
            _accountBookPolicy = accountBookPolicy;
            _periodManager = periodManager;
            _balanceOrganizer = balanceOrganizer;
            _accountBookRepository = accountBookRepository;
            _accountBookManager = accountBookManager;
        }

        public async Task<PaymentAccountDto> CreateBankAccountAsync(CreateBankAccountDto input)
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
                , input.AllowNegativeBalance
                , input.TransferAmount
                , input.FirstTransferDateTimeString.StringToNullableDateTime()
            );
            await _paymentAccountManager.CreateAsync(paymentAccount);

            await CreateAccountBookAsync(input.TransferAmount,
                input.FirstTransferDateTimeString.StringToNullableDateTime(), paymentAccount);

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
                , input.AllowNegativeBalance
                , input.TransferAmount
                , input.FirstTransferDateTimeString.StringToNullableDateTime()
            );
            await _paymentAccountManager.CreateAsync(paymentAccount);

            await CreateAccountBookAsync(input.TransferAmount,
                input.FirstTransferDateTimeString.StringToNullableDateTime(), paymentAccount);

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
                input.PersonId, input.EmployeeId, input.TenantIsOwner, existingPaymentAccount.Balance,
                input.AllowNegativeBalance, input.Iban);

            var activePeriod = await _periodManager.GetActivePeriod();
            var firstAccountBookInActivePeriod =
                await _accountBookRepository.FirstOrDefaultAsync(p => p.PeriodId == activePeriod.Id);

            await _balanceOrganizer.GetOrganizedAccountBooksAsync(
                activePeriod.StartDate, int.MaxValue,
                new List<AccountBook> { firstAccountBookInActivePeriod }, null, null);
            _balanceOrganizer.OrganizeAccountBookBalances();
            await _balanceOrganizer.OrganizePaymentAccountBalancesAsync();

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

        private async Task CreateAccountBookAsync(decimal? transferAmount, DateTime? transferProcessDateTime,
            PaymentAccount paymentAccount)
        {
            if (transferAmount.HasValue && transferProcessDateTime.HasValue)
            {
                var activePeriod = await _periodManager.GetActivePeriod();

                var accountBook = await AccountBook.CreateFirstTransferForPaymentAccountAsync(
                    _accountBookPolicy
                    , SequentialGuidGenerator.Instance.Create()
                    , AbpSession.GetTenantId()
                    , activePeriod.Id
                    , transferProcessDateTime.Value
                    , paymentAccount
                    , transferAmount.Value
                    , string.Empty
                    , new List<AccountBookFile>()
                    , AbpSession.GetUserId()
                );

                await _accountBookManager.CreateTransferForPaymentAccountAsync(accountBook, paymentAccount, true);
            }
        }
    }
}