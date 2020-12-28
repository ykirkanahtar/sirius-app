using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sirius.AppPaymentAccounts;
using Sirius.Housings;
using Sirius.People;

namespace Sirius.AccountBooks
{
    public class AccountBookPolicy : IAccountBookPolicy
    {
        private readonly IRepository<PaymentAccount, Guid> _paymentAccountRepository;

        public AccountBookPolicy(IRepository<PaymentAccount, Guid> paymentAccountRepository)
        {
            _paymentAccountRepository = paymentAccountRepository;
        }

        public async Task CheckCreateOrUpdateAttemptAsync(AccountBook accountBook, bool isUpdate,
            bool isTransferForPaymentAccount)
        {
            if (isTransferForPaymentAccount)
            {
                if (!accountBook.ToPaymentAccountId.HasValue)
                {
                    throw new UserFriendlyException("Hesap bilgisi bulunamadı.");
                }

                await _paymentAccountRepository.GetAsync(accountBook.ToPaymentAccountId.Value);
                return;
            }

            if (accountBook.FromPaymentAccountId.HasValue)
            {
                var fromPaymentAccount =
                    await _paymentAccountRepository.GetAsync(accountBook.FromPaymentAccountId.Value);
                if (fromPaymentAccount.FirstTransferDateTime.HasValue &&
                    accountBook.ProcessDateTime < fromPaymentAccount.FirstTransferDateTime.Value)
                {
                    throw new UserFriendlyException(
                        "İşletme defteri hareket tarihi, gelen hesabın ilk devir tarihinden önce olamaz.");
                }
            }

            if (accountBook.ToPaymentAccountId.HasValue)
            {
                var toPaymentAccount = await _paymentAccountRepository.GetAsync(accountBook.ToPaymentAccountId.Value);
                if (toPaymentAccount.FirstTransferDateTime.HasValue &&
                    accountBook.ProcessDateTime < toPaymentAccount.FirstTransferDateTime.Value)
                {
                    throw new UserFriendlyException(
                        "İşletme defteri hareket tarihi, giden hesabın ilk devir tarihinden önce olamaz.");
                }
            }
        }
    }
}