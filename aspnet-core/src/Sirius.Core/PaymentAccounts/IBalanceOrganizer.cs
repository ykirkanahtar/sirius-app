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

        Task GetOrganizedAccountBooksAsync(DateTime startDate, int sameDayIndex,
            List<AccountBook> createdAccountBooks, List<AccountBook> updatedAccountBooks,
            List<AccountBook> deletedAccountBooks);

        void OrganizeAccountBookBalances();
        Task OrganizePaymentAccountBalancesAsync();
    }
}