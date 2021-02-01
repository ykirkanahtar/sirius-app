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
using Sirius.Shared.Enums;

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
            PaymentAccount toPaymentAccount)
        {
            await CreateAsync(accountBook, AccountBookType.HousingDue, null, toPaymentAccount, housing);
        }

        //Yapılan ödemeyi aidattan düşme
        public async Task CreateOtherPaymentWithEncachmentForHousingDueAsync(AccountBook accountBook, Housing housing,
            [CanBeNull] PaymentAccount fromPaymentAccount, [CanBeNull] PaymentAccount toPaymentAccount)
        {
            await CreateAsync(accountBook, AccountBookType.OtherPaymentWithEncachmentForHousingDue,
                fromPaymentAccount, toPaymentAccount, housing);
        }

        public async Task CreateForPaymentAccountTransferAsync(AccountBook accountBook)
        {
            await CreateAsync(accountBook, AccountBookType.ForPaymentAccount, null, null, null);
        }

        public async Task CreateAsync(AccountBook accountBook,
            AccountBookType accountBookType,
            [CanBeNull] PaymentAccount fromPaymentAccount,
            [CanBeNull] PaymentAccount toPaymentAccount,
            [CanBeNull] Housing housing)
        {
            await accountBook.SetSameDayIndexAsync(_accountBookRepository);

            await _accountBookRepository.InsertAsync(accountBook);

            if (accountBookType == AccountBookType.ForPaymentAccount)
            {
                return;
            }

            if (accountBookType == AccountBookType.HousingDue)
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

            if (accountBookType == AccountBookType.OtherPaymentWithEncachmentForHousingDue)
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
                await OrganizePaymentAccountCurrentBalances(accountBook, fromPaymentAccount,
                    PaymentAccountDirection.From, CudType.Create);
            }

            if (toPaymentAccount != null)
            {
                await _paymentAccountManager.IncreaseBalance(toPaymentAccount, accountBook.Amount);
                await OrganizePaymentAccountCurrentBalances(accountBook, toPaymentAccount, PaymentAccountDirection.To,
                    CudType.Create);
            }
        }

        private async Task OrganizePaymentAccountCurrentBalances(AccountBook accountBook, PaymentAccount paymentAccount,
            PaymentAccountDirection paymentAccountDirection, CudType cudType,
            [CanBeNull] AccountBook oldAccountBook = null)
        {
            var amount = accountBook.Amount;
            if (cudType == CudType.Update)
            {
                if (oldAccountBook == null)
                {
                    throw new Exception("Güncelleme işlemi için eski işletme defteri değeri boş olamaz.");
                }

                amount = accountBook.Amount - oldAccountBook.Amount;
            }
            else if (cudType == CudType.Delete)
            {
                amount *= -1;
            }

            //Eğer ödeme hesabına ait son hareket değilse, ondan sonra kaydedilen hesapların bakiye bilgisi güncellenmeli

            //Giden hesabın son hesap hareketi mi 
            var nextAccountBooksForFromPaymentAccount = await _accountBookRepository.GetAll().Where(p =>
                    p.ProcessDateTime > accountBook.ProcessDateTime &&
                    p.FromPaymentAccountId == paymentAccount.Id)
                .OrderBy(p => p.ProcessDateTime)
                .ThenBy(p => p.SameDayIndex)
                .ToListAsync();

            if (nextAccountBooksForFromPaymentAccount.Count > 0)
            {
                foreach (var t in nextAccountBooksForFromPaymentAccount)
                {
                    var existingAccountBook = t;
                    var newBalance = paymentAccountDirection == PaymentAccountDirection.From
                        ? (t.FromPaymentAccountCurrentBalance ?? 0) - amount
                        : (t.FromPaymentAccountCurrentBalance ?? 0) + amount;
                    t.SetFromPaymentAccountCurrentBalance(newBalance);

                    await UpdateAsync(existingAccountBook, t);
                }
            }

            //Gelen hesabın son hesap hareketi mi 
            var nextAccountBooksForToPaymentAccount = await _accountBookRepository.GetAll().Where(p =>
                    p.ProcessDateTime > accountBook.ProcessDateTime &&
                    p.ToPaymentAccountId == paymentAccount.Id)
                .OrderBy(p => p.ProcessDateTime)
                .ThenBy(p => p.SameDayIndex)
                .ToListAsync();

            if (nextAccountBooksForToPaymentAccount.Count > 0)
            {
                foreach (var p in nextAccountBooksForToPaymentAccount)
                {
                    var existingAccountBook = p;
                    var newBalance = paymentAccountDirection == PaymentAccountDirection.From
                        ? (p.ToPaymentAccountCurrentBalance ?? 0) - amount
                        : (p.ToPaymentAccountCurrentBalance ?? 0) + amount;
                    p.SetToPaymentAccountCurrentBalance(newBalance);

                    await UpdateAsync(existingAccountBook, p);
                }
            }

            //Eğer kaydedilen ödeme hesap hareketi son hesap hareketi ise, o zaman ödeme hesabının bakiyesi kaydediliyor
            if (nextAccountBooksForFromPaymentAccount.Count == 0 && nextAccountBooksForToPaymentAccount.Count == 0)
            {
                if (paymentAccountDirection == PaymentAccountDirection.From)
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
                        p.ProcessDateTime <= accountBook.ProcessDateTime &&
                        (p.FromPaymentAccountId == paymentAccount.Id ||
                         p.ToPaymentAccountId == paymentAccount.Id))
                    .OrderByDescending(p => p.ProcessDateTime)
                    .ThenByDescending(p => p.SameDayIndex)
                    .FirstAsync();

                if (paymentAccountDirection == PaymentAccountDirection.From)
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

        public async Task UpdateAsync(AccountBook existingAccountBook, AccountBook updatedAccountBook)
        {
            try
            {
                //Tutar değişmiş mi ?
                if (updatedAccountBook.Amount != existingAccountBook.Amount)
                {
                    var amountDiff = updatedAccountBook.Amount - existingAccountBook.Amount;

                    if (updatedAccountBook.FromPaymentAccountId.HasValue)
                    {
                        if (updatedAccountBook.FromPaymentAccountId != existingAccountBook.FromPaymentAccountId)
                        {
                            throw new Exception(
                                "Ödeme hesabı değiştirme desteklenmiyor, eski işletme defteri hareketini silip, yeni işletme defteri hareketi oluşturunuz.");
                        }

                        var fromPaymentAccount =
                            await _paymentAccountManager.GetAsync(updatedAccountBook.FromPaymentAccountId.Value);

                        if (amountDiff > 0)
                        {
                            await _paymentAccountManager.DecreaseBalance(fromPaymentAccount, amountDiff);
                        }
                        else
                        {
                            await _paymentAccountManager.IncreaseBalance(fromPaymentAccount, Math.Abs(amountDiff));
                        }

                        await OrganizePaymentAccountCurrentBalances(updatedAccountBook, fromPaymentAccount,
                            PaymentAccountDirection.From, CudType.Update, existingAccountBook);
                    }

                    if (updatedAccountBook.ToPaymentAccountId.HasValue)
                    {
                        if (updatedAccountBook.ToPaymentAccountId != existingAccountBook.ToPaymentAccountId)
                        {
                            throw new Exception(
                                "Ödeme hesabı değiştirme desteklenmiyor, eski işletme defteri hareketini silip, yeni işletme defteri hareketi oluşturunuz.");
                        }

                        var toPaymentAccount =
                            await _paymentAccountManager.GetAsync(updatedAccountBook.ToPaymentAccountId.Value);

                        if (amountDiff > 0)
                        {
                            await _paymentAccountManager.IncreaseBalance(toPaymentAccount, amountDiff);
                        }
                        else
                        {
                            await _paymentAccountManager.DecreaseBalance(toPaymentAccount, Math.Abs(amountDiff));
                        }

                        await OrganizePaymentAccountCurrentBalances(updatedAccountBook, toPaymentAccount,
                            PaymentAccountDirection.To, CudType.Update, existingAccountBook);
                    }
                }

                await _accountBookRepository.UpdateAsync(updatedAccountBook);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
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

                await OrganizePaymentAccountCurrentBalances(accountBook, fromPaymentAccount,
                    PaymentAccountDirection.From, CudType.Delete);
            }

            if (accountBook.ToPaymentAccountId.HasValue)
            {
                var toPaymentAccount = await _paymentAccountManager.GetAsync(accountBook.ToPaymentAccountId.Value);
                await _paymentAccountManager.DecreaseBalance(toPaymentAccount, Math.Abs(accountBook.Amount));

                await OrganizePaymentAccountCurrentBalances(accountBook, toPaymentAccount, PaymentAccountDirection.To,
                    CudType.Delete);
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