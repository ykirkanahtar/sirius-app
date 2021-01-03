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
        Task CreateForHousingDueAsync(AccountBook accountBook, Housing housing, PaymentAccount toPaymentAccount, DbContext dbContext);

        Task CreateOtherPaymentWithEncachmentForHousingDueAsync(AccountBook accountBook, Housing housing,
            [CanBeNull] PaymentAccount fromPaymentAccount, [CanBeNull] PaymentAccount toPaymentAccount, DbContext dbContext);

        Task CreateAsync(AccountBook accountBook,
            AccountBookCreateType accountBookCreateType,
            [CanBeNull] PaymentAccount fromPaymentAccount,
            [CanBeNull] PaymentAccount toPaymentAccount,
            [CanBeNull] Housing housing,
            DbContext dbContext);
        
        Task CreateForPaymentAccountTransferAsync(AccountBook accountBook, DbContext dbContext);
        Task UpdateAsync(AccountBook accountBook);
        Task DeleteAsync(AccountBook accountBook);
        Task<AccountBook> GetAsync(Guid id);
        Task<AccountBookFile> GetAccountBookFileByUrlAsync(string url);
    }
}
