﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sirius.PaymentCategories;

namespace Sirius.PaymentAccounts
{
    public class PaymentAccountManager : IPaymentAccountManager
    {
        private readonly IRepository<PaymentAccount, Guid> _paymentAccountRepository;
        private readonly IRepository<AccountBook, Guid> _accountBookRepository;

        public PaymentAccountManager(IRepository<PaymentAccount, Guid> paymentAccountRepository,
            IRepository<AccountBook, Guid> accountBookRepository)
        {
            _paymentAccountRepository = paymentAccountRepository;
            _accountBookRepository = accountBookRepository;
        }

        public async Task CreateAsync(PaymentAccount paymentAccount)
        {
            await _paymentAccountRepository.InsertAsync(paymentAccount);
        }

        public async Task UpdateAsync(PaymentAccount paymentAccount)
        {
            await _paymentAccountRepository.UpdateAsync(paymentAccount);
        }

        public async Task DeleteAsync(PaymentAccount paymentAccount)
        {
            var accountBooks = await _accountBookRepository.GetAllListAsync(
                p => (p.FromPaymentAccountId == paymentAccount.Id
                      || p.ToPaymentAccountId == paymentAccount.Id) &&
                     p.AccountBookType != AccountBookType.FirstTransferForPaymentAccount);

            if (accountBooks.Count > 0)
            {
                throw new UserFriendlyException(
                    "Bu ödeme hesabı için bir ya da birden fazla işlem hareketi tanımlıdır. Silmek için önce tanımları kaldırınız.");
            }
            
            var transferForPaymentAccounts = await _accountBookRepository.GetAllListAsync(
                p => (p.FromPaymentAccountId == paymentAccount.Id
                      || p.ToPaymentAccountId == paymentAccount.Id) &&
                     p.AccountBookType == AccountBookType.FirstTransferForPaymentAccount);

            foreach (var transferForPaymentAccount in transferForPaymentAccounts)
            {
                await _accountBookRepository.DeleteAsync(transferForPaymentAccount);
            }
            
            await _paymentAccountRepository.DeleteAsync(paymentAccount);
        }

        public async Task<PaymentAccount> GetAsync(Guid id)
        {
            var paymentAccount = await _paymentAccountRepository.GetAsync(id);
            if (paymentAccount == null)
            {
                throw new UserFriendlyException("Hesap bulunamadı");
            }

            return paymentAccount;
        }

        public async Task IncreaseBalance(PaymentAccount paymentAccount, decimal amount)
        {
            paymentAccount = PaymentAccount.IncreaseBalance(paymentAccount, amount);
            await _paymentAccountRepository.UpdateAsync(paymentAccount);
        }

        public async Task DecreaseBalance(PaymentAccount paymentAccount, decimal amount)
        {
            paymentAccount = PaymentAccount.DecreaseBalance(paymentAccount, amount);
            await _paymentAccountRepository.UpdateAsync(paymentAccount);
        }
    }
}