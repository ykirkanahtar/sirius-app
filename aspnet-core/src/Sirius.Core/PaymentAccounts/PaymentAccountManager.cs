using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.UI;
using Sirius.AppPaymentAccounts;

namespace Sirius.PaymentAccounts
{
    public class PaymentAccountManager : IPaymentAccountManager
    {
        private readonly IRepository<PaymentAccount, Guid> _paymentAccountRepository;

        public PaymentAccountManager(IRepository<PaymentAccount, Guid> paymentAccountRepository)
        {
            _paymentAccountRepository = paymentAccountRepository;
        }

        public async Task CreateAsync(PaymentAccount paymentAccount)
        {
            await _paymentAccountRepository.InsertAsync(paymentAccount);
        }

        public async Task UpdateAsync(PaymentAccount paymentAccount)
        {
            await _paymentAccountRepository.UpdateAsync(paymentAccount);
        }
        
        public async Task<PaymentAccount> GetAsync(Guid id)
        {
            var paymentAccount = await _paymentAccountRepository.GetAsync(id);
            if(paymentAccount == null)
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
