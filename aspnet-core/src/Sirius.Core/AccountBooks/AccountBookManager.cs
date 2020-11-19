using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sirius.AppPaymentAccounts;
using Sirius.HousingPaymentPlans;
using Sirius.Housings;
using Sirius.PaymentAccounts;
using Sirius.PaymentCategories;

namespace Sirius.AccountBooks
{
    public class AccountBookManager : IAccountBookManager
    {
        private readonly IRepository<AccountBook, Guid> _accountBookRepository;
        private readonly IRepository<HousingPaymentPlan, Guid> _housingPaymentPlanRepository;
        private readonly IHousingPaymentPlanManager _housingPaymentPlanManager;
        private readonly IPaymentAccountManager _paymentAccountManager;
        
        public AccountBookManager(IRepository<AccountBook, Guid> accountBookRepository,
            IRepository<HousingPaymentPlan, Guid> housingPaymentPlanRepository,
            IHousingPaymentPlanManager housingPaymentPlanManager, 
            IPaymentAccountManager paymentAccountManager)
        {
            _accountBookRepository = accountBookRepository;
            _housingPaymentPlanRepository = housingPaymentPlanRepository;
            _housingPaymentPlanManager = housingPaymentPlanManager;
            _paymentAccountManager = paymentAccountManager;
        }

        public async Task CreateAsync(AccountBook accountBook)
        {
            await _accountBookRepository.InsertAsync(accountBook);
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
    }
}