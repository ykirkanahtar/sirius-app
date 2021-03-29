using System;
using System.Threading.Tasks;
using Abp.Domain.Services;

namespace Sirius.PaymentAccounts
{
    public interface IPaymentAccountManager : IDomainService
    {
        Task<PaymentAccount> GetAsync(Guid id);
        Task CreateAsync(PaymentAccount paymentAccount);
        Task UpdateAsync(PaymentAccount paymentAccount);
        Task DeleteAsync(PaymentAccount paymentAccount);
        Task IncreaseBalance(PaymentAccount paymentAccount, decimal amount);
        Task DecreaseBalance(PaymentAccount paymentAccount, decimal amount);
    }
}
