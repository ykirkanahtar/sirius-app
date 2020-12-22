using System;
using System.Threading.Tasks;
using Abp.Domain.Services;
using JetBrains.Annotations;
using Sirius.AppPaymentAccounts;
using Sirius.Housings;

namespace Sirius.AccountBooks
{
    public interface IAccountBookManager : IDomainService
    {
        Task CreateForHousingDueAsync(AccountBook accountBook, Housing housing, PaymentAccount toPaymentAccount);

        Task CreateOtherPaymentWithEncachmentForHousingDueAsync(AccountBook accountBook, Housing housing,
            [CanBeNull] PaymentAccount fromPaymentAccount, [CanBeNull] PaymentAccount toPaymentAccount);
        Task CreateAsync(AccountBook accountBook, [CanBeNull] PaymentAccount fromPaymentAccount,
            [CanBeNull] PaymentAccount toPaymentAccount);
        Task CreateForPaymentAccountTransferAsync(AccountBook accountBook);
        Task UpdateAsync(AccountBook accountBook);
        Task DeleteAsync(AccountBook accountBook);
        Task<AccountBook> GetAsync(Guid id);
        Task<AccountBookFile> GetAccountBookFileByUrlAsync(string url);
    }
}
