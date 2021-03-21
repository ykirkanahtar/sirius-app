using System;
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
        private readonly IPaymentCategoryManager _paymentCategoryManager;

        public PaymentAccountManager(IRepository<PaymentAccount, Guid> paymentAccountRepository,
            IRepository<AccountBook, Guid> accountBookRepository, IPaymentCategoryManager paymentCategoryManager)
        {
            _paymentAccountRepository = paymentAccountRepository;
            _accountBookRepository = accountBookRepository;
            _paymentCategoryManager = paymentCategoryManager;
        }

        public async Task CreateAsync(PaymentAccount paymentAccount)
        {
            await UnSetDefaultPaymentAccount(paymentAccount);
            await _paymentAccountRepository.InsertAsync(paymentAccount);
        }

        public async Task UpdateAsync(PaymentAccount paymentAccount)
        {
            await UnSetDefaultPaymentAccount(
                paymentAccount);
            await _paymentAccountRepository.UpdateAsync(paymentAccount);
        }

        public async Task DeleteAsync(PaymentAccount paymentAccount)
        {
            // incele
            // var transferPaymentCategory = await _paymentCategoryManager.GetTransferForPaymentAccountAsync();

            var accountBooks = await _accountBookRepository.GetAllListAsync(
                p => p.FromPaymentAccountId == paymentAccount.Id
                      || p.ToPaymentAccountId == paymentAccount.Id);
            if (accountBooks.Count > 0)
            {
                throw new UserFriendlyException(
                    "Bu ödeme hesabı için bir ya da birden fazla işlem hareketi tanımlıdır. Silmek için önce tanımları kaldırınız.");
            }

            await _paymentAccountRepository.DeleteAsync(paymentAccount);

            // var transferAccountBook =
            //     await _accountBookRepository.GetAll().Where(p => p.PaymentCategoryId == transferPaymentCategory.Id)
            //         .SingleOrDefaultAsync();
            //
            // if (transferAccountBook != null)
            // {
            //     await _accountBookRepository.DeleteAsync(transferAccountBook);
            // }
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

        public async Task UnSetDefaultPaymentAccount(PaymentAccount paymentAccount)
        {
            if (paymentAccount.IsDefault)
            {
                var defaultPaymentAccount =
                    await _paymentAccountRepository.GetAll().Where(p => p.IsDefault && p.Id != paymentAccount.Id)
                        .SingleOrDefaultAsync();

                if (defaultPaymentAccount != null)
                {
                    defaultPaymentAccount = PaymentAccount.UnSetDefaultPaymentAccount(defaultPaymentAccount);
                    await _paymentAccountRepository.UpdateAsync(defaultPaymentAccount);
                }
            }
        }
    }
}