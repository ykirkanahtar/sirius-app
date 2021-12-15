using System;
using System.Collections.Generic;
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
using Sirius.Periods;
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
        private readonly IBalanceOrganizer _balanceOrganizer;
        private readonly IPeriodManager _periodManager;

        public AccountBookManager(IRepository<AccountBook, Guid> accountBookRepository,
            IRepository<HousingPaymentPlan, Guid> housingPaymentPlanRepository,
            IHousingPaymentPlanManager housingPaymentPlanManager,
            IPaymentAccountManager paymentAccountManager,
            IRepository<AccountBookFile, Guid> accountBookFileRepository,
            IPaymentCategoryManager paymentCategoryManager,
            IHousingManager housingManager,
            IBalanceOrganizer balanceOrganizer,
            IPeriodManager periodManager)
        {
            _accountBookRepository = accountBookRepository;
            _housingPaymentPlanRepository = housingPaymentPlanRepository;
            _housingPaymentPlanManager = housingPaymentPlanManager;
            _paymentAccountManager = paymentAccountManager;
            _accountBookFileRepository = accountBookFileRepository;
            _paymentCategoryManager = paymentCategoryManager;
            _housingManager = housingManager;
            _balanceOrganizer = balanceOrganizer;
            _periodManager = periodManager;
        }

        public async Task CreateForHousingDueAsync(AccountBook accountBook, Housing housing,
            PaymentAccount toPaymentAccount, bool organizeBalances)
        {
            await CreateAsync(accountBook, null, toPaymentAccount, housing, null, null,
                organizeBalances);
        }

        //Yapılan ödemeyi aidattan düşme
        public async Task CreateOtherPaymentWithNettingForHousingDueAsync(AccountBook accountBook,
            Housing housingForNetting,
            [CanBeNull] PaymentAccount fromPaymentAccount, [CanBeNull] PaymentAccount toPaymentAccount,
            PaymentCategory paymentCategoryForNetting,
            bool organizeBalances)
        {
            await CreateAsync(accountBook,
                fromPaymentAccount, toPaymentAccount, null, housingForNetting, paymentCategoryForNetting,
                organizeBalances);
        }

        public async Task CreateTransferForPaymentAccountAsync(AccountBook accountBook, PaymentAccount toPaymentAccount,
            bool organizeBalances)
        {
            if (accountBook.AccountBookType == AccountBookType.FirstTransferForPaymentAccount ||
                accountBook.AccountBookType == AccountBookType.TransferForPaymentAccountFromPreviousPeriod ||
                accountBook.AccountBookType == AccountBookType.TransferForPaymentAccountToNextPeriod)
            {
                await CreateAsync(accountBook, null, toPaymentAccount,
                    null,
                    null, null, organizeBalances);
            }
            else
            {
                throw new Exception("Sistem hatası!!! Uyumsuz işletme defteri tipi");
            }
        }

        public async Task CreateAsync(AccountBook accountBook,
            [CanBeNull] PaymentAccount fromPaymentAccount,
            [CanBeNull] PaymentAccount toPaymentAccount,
            [CanBeNull] Housing housing,
            [CanBeNull] Housing housingForNetting,
            [CanBeNull] PaymentCategory paymentCategoryForNetting,
            bool organizeBalances)
        {
            if (fromPaymentAccount == null
                && toPaymentAccount == null
                && accountBook.NettingHousing == false)
            {
                throw new UserFriendlyException(
                    "Gelen hesap, giden hesap ya da mahsuplaşmadan en az biri seçilmelidir.");
            }

            if (accountBook.AccountBookType != AccountBookType.TransferForPaymentAccountFromPreviousPeriod &&
                accountBook.AccountBookType != AccountBookType.TransferForPaymentAccountToNextPeriod)
            {
                var activePeriod = await _periodManager.GetActivePeriod();
                if (accountBook.ProcessDateTime < activePeriod.StartDate ||
                    (activePeriod.EndDate.HasValue && accountBook.ProcessDateTime > activePeriod.EndDate.Value))
                {
                    throw new UserFriendlyException(
                        "İşletme defteri kaydı aktif dönem tarihleri içerisinde olmalıdır.");
                }
            }

            if (accountBook.SameDayIndex > 0 == false)
            {
                await accountBook.SetSameDayIndexAsync(_accountBookRepository);
            }

            await _accountBookRepository.InsertAsync(accountBook);

            if (accountBook.AccountBookType == AccountBookType.FirstTransferForPaymentAccount)
            {
                return;
            }

            //Hareket tarihi banka devir işlemlerinden önce olamaz
            if (fromPaymentAccount != null)
            {
                var transferDate = await _accountBookRepository.GetAll().Where(p =>
                        p.ToPaymentAccountId == fromPaymentAccount.Id &&
                        p.AccountBookType == AccountBookType.FirstTransferForPaymentAccount)
                    .Select(p => p.ProcessDateTime).SingleOrDefaultAsync();

                if (accountBook.ProcessDateTime < transferDate)
                {
                    throw new UserFriendlyException(
                        $"Hareket tarihi, bu ödeme hesabının {transferDate:dd-MM-yyyy} tarihli devir tarihinden önce olamaz.");
                }
            }

            if (toPaymentAccount != null)
            {
                var transferDate = await _accountBookRepository.GetAll().Where(p =>
                        p.ToPaymentAccountId == toPaymentAccount.Id &&
                        p.AccountBookType == AccountBookType.FirstTransferForPaymentAccount)
                    .Select(p => p.ProcessDateTime).SingleOrDefaultAsync();

                if (accountBook.ProcessDateTime < transferDate)
                {
                    throw new UserFriendlyException(
                        $"Hareket tarihi, bu ödeme hesabının {transferDate:dd-MM-yyyy} tarihli devir tarihinden önce olamaz.");
                }
            }

            var paymentCategory = accountBook.PaymentCategoryId.HasValue
                ? await _paymentCategoryManager.GetAsync(accountBook.PaymentCategoryId.Value)
                : null;

            if (accountBook.AccountBookType == AccountBookType.HousingDue)
            {
                await _housingManager.DecreaseBalance(housing, accountBook.Amount,
                    paymentCategory.HousingDueForResidentOrOwner.Value);

                await _housingPaymentPlanManager.CreateAsync(HousingPaymentPlan.CreateCredit(
                    SequentialGuidGenerator.Instance.Create()
                    , accountBook.TenantId
                    , housing
                    , paymentCategory.HousingDueForResidentOrOwner.Value
                    , paymentCategory
                    , accountBook.ProcessDateTime
                    , accountBook.Amount
                    , accountBook.Description
                    , accountBook
                    , HousingPaymentPlanType.HousingDuePayment
                    , null
                    , null
                ));
            }

            if (accountBook.AccountBookType == AccountBookType.OtherPaymentWithNettingForHousingDue)
            {
                await _housingManager.DecreaseBalance(housingForNetting, accountBook.Amount,
                    paymentCategoryForNetting.HousingDueForResidentOrOwner.GetValueOrDefault());

                await _housingPaymentPlanManager.CreateAsync(HousingPaymentPlan.CreateCredit(
                    SequentialGuidGenerator.Instance.Create()
                    , accountBook.TenantId
                    , housingForNetting
                    , paymentCategoryForNetting.HousingDueForResidentOrOwner.GetValueOrDefault()
                    , paymentCategoryForNetting
                    , accountBook.ProcessDateTime
                    , accountBook.Amount
                    , accountBook.Description
                    , accountBook
                    , HousingPaymentPlanType.Netting
                    , null
                    , null
                ));
            }

            if (organizeBalances)
            {
                var paymentAccounts = new List<PaymentAccount>();
                if (fromPaymentAccount != null)
                {
                    paymentAccounts.Add(fromPaymentAccount);
                }

                if (toPaymentAccount != null)
                {
                    paymentAccounts.Add(toPaymentAccount);
                }

                if (paymentAccounts.Any())
                {
                    await _balanceOrganizer.GetOrganizedAccountBooksAsync(accountBook.ProcessDateTime,
                        accountBook.SameDayIndex,
                        paymentAccounts, new List<AccountBook> { accountBook }, null, null);
                    _balanceOrganizer.OrganizeAccountBookBalances();
                    _balanceOrganizer.OrganizePaymentAccountBalances();
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
                    }
                }

                await _accountBookRepository.UpdateAsync(updatedAccountBook);

                var paymentAccounts = new List<PaymentAccount>();

                if (updatedAccountBook.FromPaymentAccountId.HasValue)
                {
                    var fromPaymentAccount =
                        await _paymentAccountManager.GetAsync(updatedAccountBook.FromPaymentAccountId.Value);
                    paymentAccounts.Add(fromPaymentAccount);
                }

                if (updatedAccountBook.ToPaymentAccountId.HasValue)
                {
                    var toPaymentAccount =
                        await _paymentAccountManager.GetAsync(updatedAccountBook.ToPaymentAccountId.Value);
                    paymentAccounts.Add(toPaymentAccount);
                }

                if (paymentAccounts.Any())
                {
                    await _balanceOrganizer.GetOrganizedAccountBooksAsync(updatedAccountBook.ProcessDateTime,
                        updatedAccountBook.SameDayIndex,
                        paymentAccounts, null, new List<AccountBook> { updatedAccountBook }, null);
                    _balanceOrganizer.OrganizeAccountBookBalances();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task DeleteAsync(AccountBook accountBook, bool organizeBalances)
        {
            var housingPaymentPlans =
                await _housingPaymentPlanRepository.GetAllListAsync(p => p.AccountBookId == accountBook.Id);

            foreach (var housingPaymentPlan in housingPaymentPlans)
            {
                await _housingPaymentPlanManager.DeleteAsync(housingPaymentPlan, true);
            }

            await _accountBookRepository.DeleteAsync(accountBook);

            if (organizeBalances)
            {
                var paymentAccounts = new List<PaymentAccount>();

                if (accountBook.FromPaymentAccountId.HasValue)
                {
                    var fromPaymentAccount =
                        await _paymentAccountManager.GetAsync(accountBook.FromPaymentAccountId.Value);
                    paymentAccounts.Add(fromPaymentAccount);
                }

                if (accountBook.ToPaymentAccountId.HasValue)
                {
                    var toPaymentAccount = await _paymentAccountManager.GetAsync(accountBook.ToPaymentAccountId.Value);
                    paymentAccounts.Add(toPaymentAccount);
                }

                if (paymentAccounts.Any())
                {
                    await _balanceOrganizer.GetOrganizedAccountBooksAsync(accountBook.ProcessDateTime, accountBook.SameDayIndex,
                        paymentAccounts, null, null, new List<AccountBook> { accountBook });
                    _balanceOrganizer.OrganizeAccountBookBalances();
                    _balanceOrganizer.OrganizePaymentAccountBalances();
                }
            }
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

        public async Task<int> GetSameDayIndexAsync(DateTime processDateTime)
        {
            return await _accountBookRepository.GetAll()
                .Where(p => p.ProcessDateTime.Year == processDateTime.Year
                            && p.ProcessDateTime.Month == processDateTime.Month
                            && p.ProcessDateTime.Day == processDateTime.Day)
                .Select(p => p.SameDayIndex)
                .OrderByDescending(p => p)
                .FirstOrDefaultAsync();
        }
    }
}