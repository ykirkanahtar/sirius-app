using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.UI;
using Sirius.Housings;

namespace Sirius.HousingPaymentPlans
{
    public class HousingPaymentPlanGroupManager : IHousingPaymentPlanGroupManager
    {
        private readonly IRepository<HousingPaymentPlanGroup, Guid> _housingPaymentPlanGroupRepository;
        private readonly IRepository<HousingPaymentPlan, Guid> _housingPaymentPlanRepository;
        private readonly IHousingPaymentPlanManager _housingPaymentPlanManager;
        private readonly IHousingManager _housingManager;

        public HousingPaymentPlanGroupManager(
            IRepository<HousingPaymentPlanGroup, Guid> housingPaymentPlanGroupRepository,
            IHousingManager housingManager,
            IRepository<HousingPaymentPlan, Guid> housingPaymentPlanRepository,
            IHousingPaymentPlanManager housingPaymentPlanManager)
        {
            _housingPaymentPlanGroupRepository = housingPaymentPlanGroupRepository;
            _housingManager = housingManager;
            _housingPaymentPlanRepository = housingPaymentPlanRepository;
            _housingPaymentPlanManager = housingPaymentPlanManager;
        }

        public async Task CreateAsync(HousingPaymentPlanGroup housingPaymentPlanGroup)
        {
            await _housingPaymentPlanGroupRepository.InsertAsync(housingPaymentPlanGroup);
        }
        public async Task UpdateAsync(HousingPaymentPlanGroup housingPaymentPlanGroup)
        {
            await _housingPaymentPlanGroupRepository.UpdateAsync(housingPaymentPlanGroup);
        }
        public async Task DeleteAsync(HousingPaymentPlanGroup housingPaymentPlanGroup)
        {
            var housingPaymentPlans =
                await _housingPaymentPlanRepository.GetAllListAsync(p =>
                    p.HousingPaymentPlanGroupId == housingPaymentPlanGroup.Id);

            foreach (var housingPaymentPlan in housingPaymentPlans)
            {
                await _housingPaymentPlanManager.DeleteAsync(housingPaymentPlan);
            }
            await _housingPaymentPlanGroupRepository.DeleteAsync(housingPaymentPlanGroup);
        }

        public async Task<HousingPaymentPlanGroup> GetAsync(Guid id)
        {
            var housingPaymentPlanGroup = await _housingPaymentPlanGroupRepository.GetAsync(id);
            if (housingPaymentPlanGroup == null)
            {
                throw new UserFriendlyException("Ödeme planı grubu bulunamadı");
            }

            return housingPaymentPlanGroup;
        }
    }
}