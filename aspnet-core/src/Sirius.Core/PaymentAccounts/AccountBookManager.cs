using System;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Domain.Repositories;
using Abp.UI;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Sirius.HousingPaymentPlans;
using Sirius.Housings;
using Sirius.PaymentCategories;

namespace Sirius.PaymentAccounts
{
    public class AccountBookManager : IAccountBookManager
    {
        private readonly IRepository<AccountBook, Guid> _accountBookRepository;
        private readonly IRepository<AccountBookFile, Guid> _accountBookFileRepository;
        private readonly IRepository<HousingPaymentPlan, Guid> _housingPaymentPlanRepository;
        private readonly IHousingPaymentPlanManager _housingPaymentPlanManager;
        private readonly IPaymentAccountManager _paymentAccountManager;
        private readonly IPaymentCategoryManager _paymentCategoryManager;
        private readonly IHousingManager _housingManager;

        public AccountBookManager(IRepository<AccountBook, Guid> accountBookRepository,
            IRepository<HousingPaymentPlan, Guid> housingPaymentPlanRepository,
            IHousingPaymentPlanManager housingPaymentPlanManager,
            IPaymentAccountManager paymentAccountManager,
            IRepository<AccountBookFile, Guid> accountBookFileRepository,
            IPaymentCategoryManager paymentCategoryManager,
            IHousingManager housingManager)
        {
            _accountBookRepository = accountBookRepository;
            _housingPaymentPlanRepository = housingPaymentPlanRepository;
            _housingPaymentPlanManager = housingPaymentPlanManager;
            _paymentAccountManager = paymentAccountManager;
            _accountBookFileRepository = accountBookFileRepository;
            _paymentCategoryManager = paymentCategoryManager;
            _housingManager = housingManager;
        }

        public async Task CreateForHousingDueAsync(AccountBook accountBook, Housing housing,
            PaymentAccount toPaymentAccount, DbContext dbContext)
        {
            await CreateAsync(accountBook, AccountBookCreateType.HousingDue, null, toPaymentAccount, housing,
                dbContext);
        }

        //Yapılan ödemeyi aidattan düşme
        public async Task CreateOtherPaymentWithEncachmentForHousingDueAsync(AccountBook accountBook, Housing housing,
            [CanBeNull] PaymentAccount fromPaymentAccount, [CanBeNull] PaymentAccount toPaymentAccount,
            DbContext dbContext)
        {
            await CreateAsync(accountBook, AccountBookCreateType.OtherPaymentWithEncachmentForHousingDue,
                fromPaymentAccount, toPaymentAccount, housing, dbContext);
        }

        public async Task CreateForPaymentAccountTransferAsync(AccountBook accountBook, DbContext dbContext)
        {
            await CreateAsync(accountBook, AccountBookCreateType.ForPaymentAccount, null, null, null, dbContext);
        }

        public async Task CreateAsync(AccountBook accountBook,
            AccountBookCreateType accountBookCreateType,
            [CanBeNull] PaymentAccount fromPaymentAccount,
            [CanBeNull] PaymentAccount toPaymentAccount,
            [CanBeNull] Housing housing,
            DbContext dbContext)
        {
            await _accountBookRepository.InsertAsync(accountBook);

            if (accountBookCreateType == AccountBookCreateType.ForPaymentAccount)
            {
                return;
            }

            if (accountBookCreateType == AccountBookCreateType.HousingDue)
            {
                var housingDuePaymentCategory = await _paymentCategoryManager.GetRegularHousingDueAsync();

                await _housingManager.DecreaseBalance(housing, accountBook.Amount);

                await _housingPaymentPlanManager.CreateAsync(HousingPaymentPlan.CreateCredit(
                    SequentialGuidGenerator.Instance.Create()
                    , accountBook.TenantId
                    , housing
                    , housingDuePaymentCategory
                    , accountBook.ProcessDateTime
                    , accountBook.Amount
                    , accountBook.Description
                    , accountBook
                ));
            }

            if (accountBookCreateType == AccountBookCreateType.OtherPaymentWithEncachmentForHousingDue)
            {
                var nettingPaymentCategory = await _paymentCategoryManager.GetNettingAsync();
                await _housingManager.DecreaseBalance(housing, accountBook.Amount);

                await _housingPaymentPlanManager.CreateAsync(HousingPaymentPlan.CreateCredit(
                    SequentialGuidGenerator.Instance.Create()
                    , accountBook.TenantId
                    , housing
                    , nettingPaymentCategory
                    , accountBook.ProcessDateTime
                    , accountBook.Amount
                    , accountBook.Description
                    , accountBook
                ));
            }

            if (fromPaymentAccount != null)
            {
                await _paymentAccountManager.DecreaseBalance(fromPaymentAccount, accountBook.Amount);
                await OrganizePaymentAccountCurrentBalances(accountBook, fromPaymentAccount, true, dbContext);
            }

            if (toPaymentAccount != null)
            {
                await _paymentAccountManager.IncreaseBalance(toPaymentAccount, accountBook.Amount);
                await OrganizePaymentAccountCurrentBalances(accountBook, toPaymentAccount, false, dbContext);
            }
        }

        private async Task OrganizePaymentAccountCurrentBalances(AccountBook accountBook, PaymentAccount paymentAccount,
            bool isFromPaymentAccount, DbContext dbContext)
        {
            try
            {
                //Eğer ödeme hesabına ait son hareket değilse, ondan sonra kaydedilen hesapların bakiye bilgisi güncellenmeli

                //Son hesap hareketi mi 
                var nextAccountBooksForFromPaymentAccount = await _accountBookRepository.GetAll().Where(p =>
                        p.ProcessDateTime > accountBook.ProcessDateTime &&
                        p.FromPaymentAccountId == paymentAccount.Id)
                    .OrderBy(p => p.ProcessDateTime)
                    .ToListAsync();

                if (nextAccountBooksForFromPaymentAccount.Count > 0)
                {
                    foreach (var t in nextAccountBooksForFromPaymentAccount)
                    {
                        var newBalance = isFromPaymentAccount
                            ? (t.FromPaymentAccountCurrentBalance ?? 0) - accountBook.Amount
                            : (t.FromPaymentAccountCurrentBalance ?? 0) + accountBook.Amount;
                        t.SetFromPaymentAccountCurrentBalance(newBalance);

                        await UpdateAsync(t);
                    }
                }

                var nextAccountBooksForToPaymentAccount = await _accountBookRepository.GetAll().Where(p =>
                        p.ProcessDateTime > accountBook.ProcessDateTime &&
                        p.ToPaymentAccountId == paymentAccount.Id)
                    .OrderBy(p => p.ProcessDateTime)
                    .ToListAsync();

                if (nextAccountBooksForToPaymentAccount.Count > 0)
                {
                    foreach (var p in nextAccountBooksForToPaymentAccount)
                    {
                        var newBalance = isFromPaymentAccount
                            ? (p.ToPaymentAccountCurrentBalance ?? 0) - accountBook.Amount
                            : (p.ToPaymentAccountCurrentBalance ?? 0) + accountBook.Amount;
                        p.SetToPaymentAccountCurrentBalance(newBalance);

                        await UpdateAsync(p);
                    }
                }

                //Eğer kaydedilen ödeme hesap hareketi son hesap hareketi ise, o zaman ödeme hesabının bakiyesi kaydediliyor
                if (nextAccountBooksForFromPaymentAccount.Count == 0 && nextAccountBooksForToPaymentAccount.Count == 0)
                {
                    if (isFromPaymentAccount)
                    {
                        accountBook.SetFromPaymentAccountCurrentBalance(paymentAccount.Balance);
                    }
                    else
                    {
                        accountBook.SetToPaymentAccountCurrentBalance(paymentAccount.Balance);
                    }
                }
                else // Değilse hesaba ait son işletme defteri hareketi bulunup o hareketteki bakiyenin üstüne tutar toplanıyor.
                {
                    var previousAccountBook = await _accountBookRepository.GetAll().Where(p =>
                            p.ProcessDateTime < accountBook.ProcessDateTime &&
                            (p.FromPaymentAccountId == paymentAccount.Id ||
                             p.ToPaymentAccountId == paymentAccount.Id))
                        .OrderByDescending(p => p.ProcessDateTime)
                        .FirstAsync();

                    if (isFromPaymentAccount)
                    {
                        var newBalance = previousAccountBook.FromPaymentAccountId == paymentAccount.Id
                            ? (previousAccountBook.FromPaymentAccountCurrentBalance ?? 0) - accountBook.Amount
                            : (previousAccountBook.ToPaymentAccountCurrentBalance ?? 0) - accountBook.Amount;
                        accountBook.SetFromPaymentAccountCurrentBalance(newBalance);
                    }
                    else
                    {
                        var newBalance = previousAccountBook.FromPaymentAccountId == paymentAccount.Id
                            ? (previousAccountBook.FromPaymentAccountCurrentBalance ?? 0) + accountBook.Amount
                            : (previousAccountBook.ToPaymentAccountCurrentBalance ?? 0) + accountBook.Amount;
                        accountBook.SetToPaymentAccountCurrentBalance(newBalance);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task UpdateAsync(AccountBook accountBook)
        {
            await _accountBookRepository.UpdateAsync(accountBook);
        }

        public async Task DeleteAsync(AccountBook accountBook)
        {
            var housingPaymentPlans =
                await _housingPaymentPlanRepository.GetAllListAsync(p => p.AccountBookId == accountBook.Id);

            foreach (var housingPaymentPlan in housingPaymentPlans)
            {
                await _housingPaymentPlanManager.DeleteAsync(housingPaymentPlan, true);
            }

            if (accountBook.FromPaymentAccountId.HasValue)
            {
                var fromPaymentAccount = await _paymentAccountManager.GetAsync(accountBook.FromPaymentAccountId.Value);
                await _paymentAccountManager.IncreaseBalance(fromPaymentAccount, Math.Abs(accountBook.Amount));
            }

            if (accountBook.ToPaymentAccountId.HasValue)
            {
                var toPaymentAccount = await _paymentAccountManager.GetAsync(accountBook.ToPaymentAccountId.Value);
                await _paymentAccountManager.DecreaseBalance(toPaymentAccount, Math.Abs(accountBook.Amount));
            }

            await _accountBookRepository.DeleteAsync(accountBook);
        }

        public async Task<AccountBook> GetAsync(Guid id)
        {
            var accountBook = await _accountBookRepository.GetAsync(id);
            if (accountBook == null)
            {
                throw new UserFriendlyException("Hesap hareketi bulunamadı");
            }

            return accountBook;
        }

        public async Task<AccountBookFile> GetAccountBookFileByUrlAsync(string url)
        {
            var accountBookFile = await _accountBookFileRepository.GetAll().Where(p => p.FileUrl == url)
                .SingleOrDefaultAsync();
            if (accountBookFile == null)
            {
                throw new UserFriendlyException("İşletme defteri görseli bulunamadı");
            }

            return accountBookFile;
        }
    }
}