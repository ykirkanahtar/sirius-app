using System;
using System.Collections.Generic;
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
        private readonly IRepository<HousingPaymentPlanGroup, Guid> _housingPaymentPlanGroupRepository;

        public PaymentCategoryManager(IRepository<PaymentCategory, Guid> paymentCategoryRepository,
            IRepository<AccountBook, Guid> accountBookRepository,
            IRepository<HousingPaymentPlan, Guid> housingPaymentPlanRepository,
            IRepository<HousingPaymentPlanGroup, Guid> housingPaymentPlanGroupRepository)
        {
            _paymentCategoryRepository = paymentCategoryRepository;
            _accountBookRepository = accountBookRepository;
            _housingPaymentPlanRepository = housingPaymentPlanRepository;
            _housingPaymentPlanGroupRepository = housingPaymentPlanGroupRepository;
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

        public async Task<PaymentCategory> GetAsync(Guid id)
        {
            var paymentCategory = await _paymentCategoryRepository.GetAsync(id);
            if (paymentCategory == null)
            {
                throw new UserFriendlyException("Kategori bulunamadı");
            }

            return paymentCategory;
        }

        public async Task<List<Guid>> GetHousingCategories(Guid paymentCategoryId)
        {
            var housingPaymentPlanGroups = await _housingPaymentPlanGroupRepository.GetAll()
                .Include(p => p.HousingPaymentPlanGroupHousingCategories)
                .Where(p => p.PaymentCategoryId == paymentCategoryId).ToListAsync();

            return housingPaymentPlanGroups
                .Select(p => p.HousingPaymentPlanGroupHousingCategories.ToList()).ToList().SelectMany(p => p)
                .ToList().Select(p => p.HousingCategoryId).ToList();
        }

        public async Task<List<Guid>> GetPaymentCategoriesByHousingCategoryIds(List<Guid> housingCategoryIds)
        {
            return await _housingPaymentPlanGroupRepository.GetAll()
                .Include(p =>
                    p.HousingPaymentPlanGroupHousingCategories)
                .Where(p => p.HousingPaymentPlanGroupHousingCategories.Any(p =>
                    housingCategoryIds.Contains(p.HousingCategoryId)))
                .Select(p => p.PaymentCategoryId).ToListAsync();
        }
    }
}