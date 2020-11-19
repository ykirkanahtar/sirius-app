using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore.Repositories;
using Abp.UI;
using Sirius.Housings;
using Sirius.Shared.Enums;

namespace Sirius.HousingPaymentPlans
{
    public class HousingPaymentPlanManager : IHousingPaymentPlanManager
    {
        private readonly IRepository<HousingPaymentPlan, Guid> _housingPaymentPlanRepository;
        private readonly IHousingManager _housingManager;

        public HousingPaymentPlanManager(IRepository<HousingPaymentPlan, Guid> housingPaymentPlanRepository, IHousingManager housingManager)
        {
            _housingPaymentPlanRepository = housingPaymentPlanRepository;
            _housingManager = housingManager;
        }

        public async Task CreateAsync(HousingPaymentPlan housingPaymentPlan)
        {
            await _housingPaymentPlanRepository.InsertAsync(housingPaymentPlan);
        }

        public async Task BulkCreateAsync(IEnumerable<HousingPaymentPlan> housingPaymentPlans)
        {
            await _housingPaymentPlanRepository.GetDbContext().AddRangeAsync(housingPaymentPlans);
        }
        
        public async Task UpdateAsync(HousingPaymentPlan housingPaymentPlan)
        {
            await _housingPaymentPlanRepository.UpdateAsync(housingPaymentPlan);
        }

        public async Task DeleteAsync(HousingPaymentPlan housingPaymentPlan)
        {
            if (housingPaymentPlan.AccountBookId.HasValue)
            {
                throw new UserFriendlyException("Bu kayıt işletme defterine girilen kayıt sonrası otomatik oluşmuştur. İşletme defterindeki kaydı siliniz.");
            }

            var housing = await _housingManager.GetAsync(housingPaymentPlan.HousingId);

            if (housingPaymentPlan.PaymentPlanType == PaymentPlanType.Debt)
            {
                await _housingManager.DecreaseBalance(housing, housingPaymentPlan.Amount);
            }
            else
            {
                await _housingManager.IncreaseBalance(housing, housingPaymentPlan.Amount);
            }

            await _housingPaymentPlanRepository.DeleteAsync(housingPaymentPlan);
        }

        public async Task<HousingPaymentPlan> GetAsync(Guid id)
        {
            var housingPaymentPlan = await _housingPaymentPlanRepository.GetAsync(id);
            if (housingPaymentPlan == null)
            {
                throw new UserFriendlyException("Ödeme planı bulunamadı");
            }
            return housingPaymentPlan;
        }
    }
}
