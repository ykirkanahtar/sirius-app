using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sirius.HousingPaymentPlans;
using Sirius.PaymentAccounts;
using Sirius.Shared.Constants;
using Sirius.Shared.Enums;

namespace Sirius.PaymentCategories
{
    public class PaymentCategoryManager : IPaymentCategoryManager
    {
        private readonly IRepository<PaymentCategory, Guid> _paymentCategoryRepository;
        private readonly IRepository<AccountBook, Guid> _accountBookRepository;
        private readonly IRepository<HousingPaymentPlan, Guid> _housingPaymentPlanRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public PaymentCategoryManager(IRepository<PaymentCategory, Guid> paymentCategoryRepository,
            IUnitOfWorkManager unitOfWorkManager, IRepository<AccountBook, Guid> accountBookRepository,
            IRepository<HousingPaymentPlan, Guid> housingPaymentPlanRepository)
        {
            _paymentCategoryRepository = paymentCategoryRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _accountBookRepository = accountBookRepository;
            _housingPaymentPlanRepository = housingPaymentPlanRepository;
        }

        public async Task CreateAsync(PaymentCategory paymentCategory)
        {
            await _paymentCategoryRepository.InsertAsync(paymentCategory);
        }

        public async Task UpdateAsync(PaymentCategory paymentCategory)
        {
            await _paymentCategoryRepository.UpdateAsync(paymentCategory);
        }

        public async Task DeleteAsync(PaymentCategory paymentCategory)
        {
            var accountBooks =
                await _accountBookRepository.GetAllListAsync(p => p.PaymentCategoryId == paymentCategory.Id);
            if (accountBooks.Count > 0)
            {
                throw new UserFriendlyException(
                    "Bu ödeme türü için bir ya da birden fazla işlem hareketi tanımlıdır. Silmek için önce tanımları kaldırınız.");
            }

            var housingPaymentPlans =
                await _housingPaymentPlanRepository.GetAllListAsync(p => p.PaymentCategoryId == paymentCategory.Id);
            if (housingPaymentPlans.Count > 0)
            {
                throw new UserFriendlyException(
                    "Bu ödeme türü için bir ya da birden fazla aidat ödeme planı tanımlıdır. Silmek için önce tanımları kaldırınız.");
            }

            await _paymentCategoryRepository.DeleteAsync(paymentCategory);
        }

        // public async Task<PaymentCategory> GetRegularHousingDueAsync()
        // {
        //     return await _paymentCategoryRepository.SingleAsync(p =>
        //         p.HousingDueType == HousingDueType.RegularHousingDue && p.IsActive);
        // }

        // public async Task<PaymentCategory> GetTransferForRegularHousingDueAsync()
        // {
        //     return await _paymentCategoryRepository.SingleAsync(p =>
        //         p.HousingDueType == HousingDueType.TransferForRegularHousingDue && p.IsActive);
        // }
        //
        // public async Task<PaymentCategory> GetNettingAsync()
        // {
        //     return await _paymentCategoryRepository.SingleAsync(p =>
        //         p.HousingDueType == HousingDueType.Netting && p.IsActive);
        // }

        // public async Task<PaymentCategory> GetTransferForPaymentAccountAsync()
        // {
        //     return await _paymentCategoryRepository.GetAll().Where(p =>
        //             p.PaymentCategoryName ==
        //             AppConstants.TransferForPaymentAccount && p.IsActive)
        //         .SingleOrDefaultAsync();
        // }

        public async Task<PaymentCategory> GetAsync(Guid id)
        {
            var paymentCategory = await _paymentCategoryRepository.GetAsync(id);
            if (paymentCategory == null)
            {
                throw new UserFriendlyException("Kategori bulunamadı");
            }

            return paymentCategory;
        }
    }
}