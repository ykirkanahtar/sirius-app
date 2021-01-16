using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Localization;
using Abp.Localization.Sources;
using Abp.UI;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Sirius.Shared.Constants;

namespace Sirius.PaymentAccounts
{
    public class AccountBookPolicy : IAccountBookPolicy
    {
        private readonly IRepository<AccountBook, Guid> _accountBookRepository;
        private readonly ILocalizationSource _localizationSource;

        public AccountBookPolicy(IRepository<AccountBook, Guid> accountBookRepository,
            ILocalizationManager localizationManager)
        {
            _accountBookRepository = accountBookRepository;
            _localizationSource =  localizationManager.GetSource(AppConstants.LocalizationSourceName);
        }

        public async Task CheckForTransferForPaymentAccountAsync(AccountBook accountBook,
            PaymentAccount toPaymentAcccount)
        {
            var firstAccountBookForToPaymentAccount = await _accountBookRepository.GetAll()
                .Where(p => p.ToPaymentAccountId == toPaymentAcccount.Id).OrderBy(p => p.ProcessDateTime)
                .FirstOrDefaultAsync();

            if (firstAccountBookForToPaymentAccount != null)
            {
                if (accountBook.ProcessDateTime > firstAccountBookForToPaymentAccount.ProcessDateTime)
                {
                    throw new UserFriendlyException(
                        $"İlk devir tarihi, bu hesaba ait ilk hareket olan {firstAccountBookForToPaymentAccount.ProcessDateTime:dd-MM-yyyy} tarihinden sonra olamaz..");
                }
            }
        }

        public void CheckCreateOrUpdateAttempt(
            AccountBook accountBook,
            [CanBeNull] PaymentAccount fromPaymentAccount,
            [CanBeNull] PaymentAccount toPaymentAccount,
            bool isUpdate
        )
        {
            CheckSamePaymentAccountId(accountBook);

            CheckPaymentAccountProcessDateWithTransferDate(accountBook, fromPaymentAccount,
                PaymentAccountDirection.From);
            CheckPaymentAccountProcessDateWithTransferDate(accountBook, toPaymentAccount,
                PaymentAccountDirection.To);

            if (isUpdate)
            {
                CheckPaymentAccountsForUpdate(accountBook.FromPaymentAccountId, fromPaymentAccount,
                    PaymentAccountDirection.From);

                CheckPaymentAccountsForUpdate(accountBook.ToPaymentAccountId, toPaymentAccount,
                    PaymentAccountDirection.To);
            }
        }

        private void CheckPaymentAccountProcessDateWithTransferDate(AccountBook accountBook,
            [CanBeNull] PaymentAccount paymentAccount, PaymentAccountDirection paymentAccountDirection)
        {
            if (paymentAccount != null)
            {
                if (paymentAccount.FirstTransferDateTime.HasValue &&
                    accountBook.ProcessDateTime < paymentAccount.FirstTransferDateTime.Value)
                {
                    throw new UserFriendlyException(
                        $"İşletme defteri hareket tarihi, {_localizationSource.GetString(paymentAccountDirection.ToString())} hesabın ilk devir tarihinden önce olamaz.");
                }
            }
        }

        private void CheckSamePaymentAccountId(AccountBook accountBook)
        {
            if (accountBook.FromPaymentAccountId.HasValue && accountBook.ToPaymentAccountId.HasValue)
            {
                if (accountBook.FromPaymentAccountId.Value == accountBook.ToPaymentAccountId.Value)
                {
                    throw new UserFriendlyException(
                        "Gelen hesap ile giden hesap aynı olamaz..");
                }
            }
        }

        private void CheckPaymentAccountsForUpdate(Guid? existingAccountBookId,
            [CanBeNull] PaymentAccount paymentAccount, PaymentAccountDirection paymentAccountDirection)
        {
            if (!existingAccountBookId.HasValue) return;
            if (paymentAccount == null)
            {
                throw new UserFriendlyException(
                    $"{_localizationSource.GetString(paymentAccountDirection.ToString())} hesap alanı güncellenemez. Kaydı silip tekrar oluşturun..");
            }

            if (existingAccountBookId.Value != paymentAccount.Id)
            {
                throw new UserFriendlyException(
                    $"{_localizationSource.GetString(paymentAccountDirection.ToString())} hesap alanı güncellenemez. Kaydı silip tekrar oluşturun..");
            }
        }
    }
}