using System.Threading.Tasks;
using Abp.Domain.Services;
using JetBrains.Annotations;

namespace Sirius.PaymentAccounts
{
    public interface IAccountBookPolicy : IDomainService
    {
        void CheckCreateOrUpdateAttempt(AccountBook accountBook, [CanBeNull] PaymentAccount fromPaymentAccount,
            [CanBeNull] PaymentAccount toPaymentAccount, bool isUpdate);

        Task CheckForTransferForPaymentAccountAsync(AccountBook accountBook,
            PaymentAccount toPaymentAcccount);
    }
}