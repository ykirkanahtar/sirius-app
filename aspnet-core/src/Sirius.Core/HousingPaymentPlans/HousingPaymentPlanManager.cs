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
        private readonly IRepository<HousingPaymentPlanGroup, Guid> _housingPaymentPlanGroupRepository;
        private readonly IRepository<Housing, Guid> _housingRepository;
        private readonly IHousingManager _housingManager;

        public HousingPaymentPlanManager(IRepository<HousingPaymentPlan, Guid> housingPaymentPlanRepository,
            IHousingManager housingManager, IRepository<Housing, Guid> housingRepository,
            IRepository<HousingPaymentPlanGroup, Guid> housingPaymentPlanGroupRepository)
        {
            _housingPaymentPlanRepository = housingPaymentPlanRepository;
            _housingManager = housingManager;
            _housingRepository = housingRepository;
            _housingPaymentPlanGroupRepository = housingPaymentPlanGroupRepository;
        }

        public async Task CreateAsync(HousingPaymentPlan housingPaymentPlan)
        {
            await _housingPaymentPlanRepository.InsertAsync(housingPaymentPlan);
        }

        public async Task BulkCreateAsync(IEnumerable<HousingPaymentPlan> housingPaymentPlans)
        {
            await _housingPaymentPlanRepository.GetDbContext().AddRangeAsync(housingPaymentPlans);
        }

        public async Task<HousingPaymentPlan> UpdateAsync(Guid housingPaymentPlanId, DateTime date,
            decimal amount, string description)
        {
            var existingHousingPaymentPlan = await _housingPaymentPlanRepository.GetAsync(housingPaymentPlanId);

            var housingPaymentPlanGroup =
                await _housingPaymentPlanGroupRepository.GetAsync(existingHousingPaymentPlan.HousingPaymentPlanGroupId
                    .GetValueOrDefault());

            var amountDiff = existingHousingPaymentPlan.CreditOrDebt == CreditOrDebt.Credit
                ? Math.Abs(existingHousingPaymentPlan.Amount) - Math.Abs(amount)
                : Math.Abs(amount) - Math.Abs(existingHousingPaymentPlan.Amount);

            var updatedHousingPaymentPlan = HousingPaymentPlan.Update(existingHousingPaymentPlan, date, amount,
                existingHousingPaymentPlan.Description);

            if (amountDiff != 0)
            {
                var housing = await _housingRepository.GetAsync(existingHousingPaymentPlan.HousingId);
                if (existingHousingPaymentPlan.CreditOrDebt == CreditOrDebt.Debt)
                {
                    if (amountDiff > 0)
                        await _housingManager.DecreaseBalance(housing, Math.Abs(amountDiff),
                            housingPaymentPlanGroup.ResidentOrOwner);
                    else
                        await _housingManager.IncreaseBalance(housing, Math.Abs(amountDiff),
                            housingPaymentPlanGroup.ResidentOrOwner);
                }
                else
                {
                    if (amountDiff > 0)
                        await _housingManager.IncreaseBalance(housing, Math.Abs(amountDiff),
                            housingPaymentPlanGroup.ResidentOrOwner);
                    else
                        await _housingManager.DecreaseBalance(housing, Math.Abs(amountDiff),
                            housingPaymentPlanGroup.ResidentOrOwner);
                }
            }

            return updatedHousingPaymentPlan;
        }

        public async Task DeleteAsync(HousingPaymentPlan housingPaymentPlan, bool fromAccountBook = false)
        {
            if (!fromAccountBook && housingPaymentPlan.AccountBookId.HasValue)
            {
                throw new UserFriendlyException(
                    "Bu kayıt işletme defterine girilen kayıt sonrası otomatik oluşmuştur. İşletme defterindeki kaydı siliniz.");
            }

            var housingPaymentPlanGroup = await _housingPaymentPlanGroupRepository.GetAsync(housingPaymentPlan.Id);
            var housing = await _housingManager.GetAsync(housingPaymentPlan.HousingId);

            if (housingPaymentPlan.CreditOrDebt == CreditOrDebt.Debt)
            {
                await _housingManager.DecreaseBalance(housing, Math.Abs(housingPaymentPlan.Amount),
                    housingPaymentPlanGroup.ResidentOrOwner);
            }
            else
            {
                await _housingManager.IncreaseBalance(housing, Math.Abs(housingPaymentPlan.Amount),
                    housingPaymentPlanGroup.ResidentOrOwner);
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