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
        Task CreateForHousingDueAsync(AccountBook accountBook, Housing housing, PaymentAccount toPaymentAccount,
            bool organizeBalances);

        Task CreateOtherPaymentWithNettingForHousingDueAsync(AccountBook accountBook, Housing housingForNetting,
            [CanBeNull] PaymentAccount fromPaymentAccount, [CanBeNull] PaymentAccount toPaymentAccount,
            PaymentCategory paymentCategoryForNetting,
            bool organizeBalances);

        Task CreateForPaymentAccountTransferAsync(AccountBook accountBook, PaymentAccount toPaymentAccount,
            bool organizeBalances);

        Task CreateAsync(AccountBook accountBook,
            AccountBookType accountBookType,
            [CanBeNull] PaymentAccount fromPaymentAccount,
            [CanBeNull] PaymentAccount toPaymentAccount,
            [CanBeNull] Housing housing,
            [CanBeNull] Housing housingForNetting,
            [CanBeNull] PaymentCategory paymentCategoryForNetting,
            bool organizeBalances);

        Task UpdateAsync(AccountBook existingAccountBook, AccountBook accountBook);
        Task DeleteAsync(AccountBook accountBook, bool organizeBalances);
        Task<AccountBook> GetAsync(Guid id);
        Task<AccountBookFile> GetAccountBookFileByUrlAsync(string url);
    }
}