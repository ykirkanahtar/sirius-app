using System;
using System.Threading.Tasks;
using Abp.Domain.Services;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Sirius.Housings;

namespace Sirius.PaymentAccounts
{
    public interface IAccountBookManager : IDomainService
    {
        Task CreateForHousingDueAsync(AccountBook accountBook, Housing housing, PaymentAccount toPaymentAccount);

        Task CreateOtherPaymentWithEncachmentForHousingDueAsync(AccountBook accountBook, Housing housing,
            [CanBeNull] PaymentAccount fromPaymentAccount, [CanBeNull] PaymentAccount toPaymentAccount);

        Task CreateAsync(AccountBook accountBook,
            AccountBookType accountBookType,
            [CanBeNull] PaymentAccount fromPaymentAccount,
            [CanBeNull] PaymentAccount toPaymentAccount,
            [CanBeNull] Housing housing);
        
        Task CreateForPaymentAccountTransferAsync(AccountBook accountBook);
        Task UpdateAsync(AccountBook existingAccountBook, AccountBook accountBook);
        Task DeleteAsync(AccountBook accountBook);
        Task<AccountBook> GetAsync(Guid id);
        Task<AccountBookFile> GetAccountBookFileByUrlAsync(string url);
    }
}
