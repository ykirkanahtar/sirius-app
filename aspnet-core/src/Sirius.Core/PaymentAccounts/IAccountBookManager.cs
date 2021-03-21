using System;
using System.Threading.Tasks;
using Abp.Domain.Services;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Sirius.Housings;
using Sirius.PaymentCategories;

namespace Sirius.PaymentAccounts
{
    public interface IAccountBookManager : IDomainService
    {
        Task CreateForHousingDueAsync(AccountBook accountBook, Housing housing, PaymentAccount toPaymentAccount);

        Task CreateOtherPaymentWithEncachmentForHousingDueAsync(AccountBook accountBook, Housing housingForEncashment,
            [CanBeNull] PaymentAccount fromPaymentAccount, [CanBeNull] PaymentAccount toPaymentAccount, PaymentCategory paymentCategoryForEncashment);

        Task CreateAsync(AccountBook accountBook,
            AccountBookType accountBookType,
            [CanBeNull] PaymentAccount fromPaymentAccount,
            [CanBeNull] PaymentAccount toPaymentAccount,
            [CanBeNull] Housing housing,
            [CanBeNull] Housing housingForEncashment,
            [CanBeNull] PaymentCategory paymentCategoryForEncashment);
        
        Task CreateForPaymentAccountTransferAsync(AccountBook accountBook);
        Task UpdateAsync(AccountBook existingAccountBook, AccountBook accountBook);
        Task DeleteAsync(AccountBook accountBook);
        Task<AccountBook> GetAsync(Guid id);
        Task<AccountBookFile> GetAccountBookFileByUrlAsync(string url);
    }
}
