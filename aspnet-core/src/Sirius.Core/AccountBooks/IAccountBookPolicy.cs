using System.Threading.Tasks;
using Abp.Domain.Services;

namespace Sirius.AccountBooks
{
    public interface IAccountBookPolicy : IDomainService
    {
        Task CheckCreateOrUpdateAttemptAsync(AccountBook block, bool isUpdate, bool isTransferForPaymentAccount);
    }
}