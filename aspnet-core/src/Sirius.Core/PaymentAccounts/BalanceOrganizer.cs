using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using Sirius.Periods;

namespace Sirius.PaymentAccounts
{
    public class BalanceOrganizer : IBalanceOrganizer
    {
        private readonly IRepository<AccountBook, Guid> _accountBookRepository;
        private readonly IRepository<PaymentAccount, Guid> _paymentAccountRepository;
        private readonly IPeriodManager _periodManager;

        public BalanceOrganizer(IRepository<AccountBook, Guid> accountBookRepository,
            IRepository<PaymentAccount, Guid> paymentAccountRepository, IPeriodManager periodManager)
        {
            _accountBookRepository = accountBookRepository;
            _paymentAccountRepository = paymentAccountRepository;
            _periodManager = periodManager;
            UpdatingAccountBooks = new List<AccountBook>();
            DeletingAccountBooks = new List<AccountBook>();
            PreviousAccountBooks = new List<AccountBook>();
        }

        public List<AccountBook> UpdatingAccountBooks { get; private set; }
        public List<AccountBook> DeletingAccountBooks { get; private set; }
        public List<AccountBook> PreviousAccountBooks { get; private set; }

        public async Task GetOrganizedAccountBooksAsync(DateTime startDate, int sameDayIndex,
            List<AccountBook> createdAccountBooks, List<AccountBook> updatedAccountBooks,
            List<AccountBook> deletedAccountBooks)
        {
            var activePeriod = await _periodManager.GetActivePeriod();

            createdAccountBooks ??= new List<AccountBook>();
            updatedAccountBooks ??= new List<AccountBook>();
            deletedAccountBooks ??= new List<AccountBook>();

            var deletedAccountBookIds = deletedAccountBooks.Select(p => p.Id).ToList();
            var updatedAccountBookIds = updatedAccountBooks.Select(p => p.Id).ToList();

            //UpdatedAccountBooks için, veritabanındaki güncel olmayan kayıtlar sorguya dahil edilmeyip, updatedAccountsBooks kayıtları accountBooks listesine ekleniyor

            var accountBooks = await _accountBookRepository.GetAll()
                .Include(p => p.FromPaymentAccount).Include(p => p.ToPaymentAccount)
                .Where(p =>
                    p.PeriodId == activePeriod.Id &&
                    (p.ProcessDateTime >= startDate) &&
                    deletedAccountBookIds.Contains(p.Id) == false &&
                    updatedAccountBookIds.Contains(p.Id) == false)
                .ToListAsync();

            accountBooks.AddRange(createdAccountBooks);
            accountBooks.AddRange(updatedAccountBooks);

            if (deletedAccountBooks.Any())
            {
                DeletingAccountBooks = deletedAccountBooks;
            }

            UpdatingAccountBooks = accountBooks
                .OrderBy(p => p.ProcessDateTime)
                .ThenBy(p => p.SameDayIndex)
                .ToList();

            /*Düzeltilecek aralıkta, tüm ödeme kategorilerinin bir önceki kayıtları olmayabilir,
             bu yüzden sistemdeki bütüm ödeme kategorilerinin listesi çekiliyor.*/
            var allPaymentAccountBookIds = await _paymentAccountRepository.GetAll().Select(p => p.Id).ToListAsync();

            foreach (var paymentAccountId in allPaymentAccountBookIds)
            {
                var previousAccountBook = await _accountBookRepository.GetAll().Where(p =>
                        p.PeriodId == activePeriod.Id &&
                        PreviousAccountBooks.Select(x => x.Id).Contains(p.Id) == false &&
                        UpdatingAccountBooks.Select(x => x.Id).Contains(p.Id) == false &&
                        (p.ProcessDateTime < startDate ||
                         (p.ProcessDateTime == startDate && p.SameDayIndex < sameDayIndex)) &&
                        (p.FromPaymentAccountId == paymentAccountId ||
                         p.ToPaymentAccountId == paymentAccountId))
                    .WhereIf(updatedAccountBooks.Any(),
                        p => updatedAccountBooks.Select(x => x.Id).Contains(p.Id) == false)
                    .WhereIf(deletedAccountBooks.Any(),
                        p => deletedAccountBooks.Select(x => x.Id).Contains(p.Id) == false)
                    .OrderByDescending(p => p.ProcessDateTime)
                    .ThenByDescending(p => p.SameDayIndex)
                    .FirstOrDefaultAsync();
                if (previousAccountBook != null)
                {
                    PreviousAccountBooks.Add(previousAccountBook);
                }
            }
        }

        public void OrganizeAccountBookBalances()
        {
            var allAccountBooks = UpdatingAccountBooks.Union(PreviousAccountBooks).OrderBy(p => p.ProcessDateTime)
                .ThenBy(p => p.SameDayIndex).ToList();

            var lastAccountBookHasDeleted = false;

            //Eğer silinen hareket en son hareket ise UpdatingAccountBooks'ta kayıt olmaz bu yüzden aşağıdaki kod ile silinen hareketin bakiyesi düşülecek
            if (UpdatingAccountBooks.Any() == false && DeletingAccountBooks.Any())
            {
                lastAccountBookHasDeleted = true;
                UpdatingAccountBooks = DeletingAccountBooks.ToList();
            }

            foreach (var updatingAccountBook in UpdatingAccountBooks)
            {
                var fromPaymentAccount = updatingAccountBook.FromPaymentAccount;
                var toPaymentAccount = updatingAccountBook.ToPaymentAccount;

                var updatingAmount = lastAccountBookHasDeleted == false ? updatingAccountBook.Amount : 0;

                if (fromPaymentAccount != null)
                {
                    var previousAccountBook = allAccountBooks.Where(p =>
                            (p.ProcessDateTime < updatingAccountBook.ProcessDateTime ||
                             (p.ProcessDateTime == updatingAccountBook.ProcessDateTime &&
                              p.SameDayIndex < updatingAccountBook.SameDayIndex))
                            &&
                            (p.FromPaymentAccountId == fromPaymentAccount.Id ||
                             p.ToPaymentAccountId == fromPaymentAccount.Id))
                        .OrderByDescending(p => p.ProcessDateTime)
                        .ThenByDescending(p => p.SameDayIndex)
                        .FirstOrDefault();

                    if (previousAccountBook == null
                       ) //Kaydedilen ödeme hesap hareketi ilk hesap hareketi ise, ödeme tutarı bakiyeniin üstüne yazılıyor
                    {
                        updatingAccountBook.SetFromPaymentAccountCurrentBalance(fromPaymentAccount,
                            updatingAmount);
                    }
                    else
                    {
                        var newBalance = previousAccountBook.FromPaymentAccountId == fromPaymentAccount.Id
                            ? (previousAccountBook.FromPaymentAccountCurrentBalance ?? 0) - updatingAmount
                            : (previousAccountBook.ToPaymentAccountCurrentBalance ?? 0) - updatingAmount;
                        updatingAccountBook.SetFromPaymentAccountCurrentBalance(fromPaymentAccount, newBalance);
                    }
                }

                if (toPaymentAccount != null)
                {
                    var previousAccountBook = allAccountBooks.Where(p =>
                            (p.ProcessDateTime < updatingAccountBook.ProcessDateTime ||
                             (p.ProcessDateTime == updatingAccountBook.ProcessDateTime &&
                              p.SameDayIndex < updatingAccountBook.SameDayIndex))
                            &&
                            (p.FromPaymentAccountId == toPaymentAccount.Id ||
                             p.ToPaymentAccountId == toPaymentAccount.Id))
                        .OrderByDescending(p => p.ProcessDateTime)
                        .ThenByDescending(p => p.SameDayIndex)
                        .FirstOrDefault();

                    if (previousAccountBook == null
                       ) //Kaydedilen ödeme hesap hareketi ilk hesap hareketi ise, ödeme tutarı bakiyeniin üstüne yazılıyor
                    {
                        updatingAccountBook.SetToPaymentAccountCurrentBalance(toPaymentAccount,
                            updatingAmount);
                    }
                    else
                    {
                        var newBalance = previousAccountBook.FromPaymentAccountId == toPaymentAccount.Id
                            ? (previousAccountBook.FromPaymentAccountCurrentBalance ?? 0) + updatingAmount
                            : (previousAccountBook.ToPaymentAccountCurrentBalance ?? 0) + updatingAmount;
                        updatingAccountBook.SetToPaymentAccountCurrentBalance(toPaymentAccount, newBalance);
                    }
                }
            }
        }

        public async Task OrganizePaymentAccountBalancesAsync()
        {
            var paymentAccounts = await _paymentAccountRepository.GetAllListAsync();
            foreach (var paymentAccount in paymentAccounts)
            {
                var lastAccountBookForFromPaymentAccount = UpdatingAccountBooks
                    .Where(p => p.FromPaymentAccountId == paymentAccount.Id)
                    .OrderByDescending(p => p.ProcessDateTime)
                    .ThenByDescending(p => p.SameDayIndex)
                    .FirstOrDefault();

                var lastAccountBookForToPaymentAccount = UpdatingAccountBooks
                    .Where(p => p.ToPaymentAccountId == paymentAccount.Id)
                    .OrderByDescending(p => p.ProcessDateTime)
                    .ThenByDescending(p => p.SameDayIndex)
                    .FirstOrDefault();

                if (lastAccountBookForFromPaymentAccount != null && lastAccountBookForToPaymentAccount != null)
                {
                    if (lastAccountBookForFromPaymentAccount.ProcessDateTime ==
                        lastAccountBookForToPaymentAccount.ProcessDateTime)
                    {
                        paymentAccount.SetBalance(
                            lastAccountBookForFromPaymentAccount.SameDayIndex >
                            lastAccountBookForToPaymentAccount.SameDayIndex
                                ? lastAccountBookForFromPaymentAccount.FromPaymentAccountCurrentBalance ?? 0
                                : lastAccountBookForToPaymentAccount.ToPaymentAccountCurrentBalance ?? 0);
                    }
                    else
                    {
                        paymentAccount.SetBalance(
                            lastAccountBookForFromPaymentAccount.ProcessDateTime >
                            lastAccountBookForToPaymentAccount.ProcessDateTime
                                ? lastAccountBookForFromPaymentAccount.FromPaymentAccountCurrentBalance ?? 0
                                : lastAccountBookForToPaymentAccount.ToPaymentAccountCurrentBalance ?? 0);
                    }
                }

                if (lastAccountBookForFromPaymentAccount != null && lastAccountBookForToPaymentAccount == null)
                {
                    paymentAccount.SetBalance(
                        lastAccountBookForFromPaymentAccount.FromPaymentAccountCurrentBalance ?? 0);
                }

                if (lastAccountBookForFromPaymentAccount == null && lastAccountBookForToPaymentAccount != null)
                {
                    paymentAccount.SetBalance(lastAccountBookForToPaymentAccount.ToPaymentAccountCurrentBalance ?? 0);
                }
            }
        }
    }
}