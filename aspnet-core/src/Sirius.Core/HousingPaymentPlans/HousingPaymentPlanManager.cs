﻿using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.UI;

namespace Sirius.HousingPaymentPlans
{
    public class HousingPaymentPlanManager : IHousingPaymentPlanManager
    {
        private readonly IRepository<HousingPaymentPlan, Guid> _housingPaymentPlanRepository;

        public HousingPaymentPlanManager(IRepository<HousingPaymentPlan, Guid> housingPaymentPlanRepository)
        {
            _housingPaymentPlanRepository = housingPaymentPlanRepository;
        }

        public async Task CreateAsync(HousingPaymentPlan housingPaymentPlan)
        {
            await _housingPaymentPlanRepository.InsertAsync(housingPaymentPlan);
        }
        
        public async Task UpdateAsync(HousingPaymentPlan housingPaymentPlan)
        {
            await _housingPaymentPlanRepository.UpdateAsync(housingPaymentPlan);
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
