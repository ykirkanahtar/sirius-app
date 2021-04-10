using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Services;

namespace Sirius.PaymentAccounts
{
    public interface IBalanceOrganizer : IDomainService
    {
        List<AccountBook> UpdatingAccountBooks { get; }
        List<AccountBook> PreviousAccountBooks { get; }
        List<PaymentAccount> PaymentAccounts { get; }

        Task GetOrganizedAccountBooksAsync(DateTime startDate,
            List<PaymentAccount> paymentAccounts, List<AccountBook> createdAccountBooks,
            List<AccountBook> updatedAccountBooks, List<AccountBook> deletedAccountBooks);

        void OrganizeAccountBookBalances();
        void OrganizePaymentAccountBalances();
    }
}