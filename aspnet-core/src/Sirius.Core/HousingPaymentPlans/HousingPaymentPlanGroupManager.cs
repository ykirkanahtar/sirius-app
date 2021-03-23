using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp;
using Abp.Domain.Repositories;
using Abp.Timing;
using Abp.UI;
using Sirius.Housings;
using Sirius.PaymentCategories;
using Sirius.Shared.Enums;

namespace Sirius.HousingPaymentPlans
{
    public class HousingPaymentPlanGroupManager : IHousingPaymentPlanGroupManager
    {
        private readonly IRepository<HousingPaymentPlanGroup, Guid> _housingPaymentPlanGroupRepository;
        private readonly IRepository<HousingPaymentPlan, Guid> _housingPaymentPlanRepository;
        private readonly IHousingPaymentPlanManager _housingPaymentPlanManager;
        private readonly IHousingManager _housingManager;
        private readonly IPaymentCategoryManager _paymentCategoryManager;

        public HousingPaymentPlanGroupManager(
            IRepository<HousingPaymentPlanGroup, Guid> housingPaymentPlanGroupRepository,
            IHousingManager housingManager,
            IRepository<HousingPaymentPlan, Guid> housingPaymentPlanRepository,
            IHousingPaymentPlanManager housingPaymentPlanManager, IPaymentCategoryManager paymentCategoryManager)
        {
            _housingPaymentPlanGroupRepository = housingPaymentPlanGroupRepository;
            _housingManager = housingManager;
            _housingPaymentPlanRepository = housingPaymentPlanRepository;
            _housingPaymentPlanManager = housingPaymentPlanManager;
            _paymentCategoryManager = paymentCategoryManager;
        }

        public async Task CreateAsync(HousingPaymentPlanGroup housingPaymentPlanGroup, List<Housing> housings,
            DateTime startDate, PaymentCategory paymentCategory, bool transferFromPreviousPeriod,
            DateTime? periodStartDate)
        {
            var newStartDate = GetStartDate(startDate, housingPaymentPlanGroup.PaymentDayOfMonth);

            await _housingPaymentPlanGroupRepository.InsertAsync(housingPaymentPlanGroup);

            var housingPaymentPlans = new List<HousingPaymentPlan>();

            foreach (var housing in housings)
            {
                if (transferFromPreviousPeriod && periodStartDate.HasValue)
                {
                    if (housing.Balance != 0)
                    {
                        // var paymentCategoryForHousingDueTransfer = await _paymentCategoryManager.GetTransferForRegularHousingDueAsync();

                        var housingPaymentPlanForHousingDueTransfer = housing.Balance > 0 //Site alacaklı ise
                            ? HousingPaymentPlan.CreateDebt(
                                SequentialGuidGenerator.Instance.Create()
                                , housingPaymentPlanGroup.TenantId
                                , null
                                , housing
                                , paymentCategory
                                , periodStartDate.Value
                                , housing.Balance
                                , string.Empty //Description
                                , HousingPaymentPlanType.Transfer
                                , null
                                , null
                            )
                            : HousingPaymentPlan.CreateCredit(
                                SequentialGuidGenerator.Instance.Create()
                                , housingPaymentPlanGroup.TenantId
                                , housing
                                , paymentCategory
                                , periodStartDate.Value
                                , housing.Balance
                                , string.Empty //Description
                                , null
                                , HousingPaymentPlanType.Transfer
                                , null
                                , null
                            );

                        housingPaymentPlans.Add(housingPaymentPlanForHousingDueTransfer);
                    }
                }

                var date = newStartDate;
                for (var i = 0; i < housingPaymentPlanGroup.CountOfMonth; i++)
                {
                    if (i > 0)
                    {
                        var day = housingPaymentPlanGroup.PaymentDayOfMonth;
                        var month = date.Month == 12 ? 1 : date.Month + 1;
                        var year = month == 1 ? date.Year + 1 : date.Year;
                        date = new DateTime(year, month, day);
                    }

                    var housingPaymentPlan = HousingPaymentPlan.CreateDebt(
                        SequentialGuidGenerator.Instance.Create()
                        , housingPaymentPlanGroup.TenantId
                        , housingPaymentPlanGroup
                        , housing
                        , paymentCategory
                        , date
                        , housingPaymentPlanGroup.AmountPerMonth
                        , housingPaymentPlanGroup.Description
                        , HousingPaymentPlanType.HousingDueDefinition
                        , null
                        , null
                    );

                    housingPaymentPlans.Add(housingPaymentPlan);
                }
            }

            await _housingPaymentPlanManager.BulkCreateAsync(housingPaymentPlans);
            _housingManager.BulkIncreaseBalance(housings,
                housingPaymentPlanGroup.AmountPerMonth * housingPaymentPlanGroup.CountOfMonth,
                housingPaymentPlanGroup.ResidentOrOwner);
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

        public DateTime GetStartDate(DateTime startDate, int paymentDayOfMonth)
        {
            var newStartDate = startDate > Clock.Now ? startDate : Clock.Now;

            if (newStartDate.Day > paymentDayOfMonth)
            {
                var year = newStartDate.Year;
                var month = newStartDate.Month == 12 ? 1 : newStartDate.Month + 1;
                year = month == 1 ? year + 1 : year;
                newStartDate = new DateTime(year, month, paymentDayOfMonth);
            }

            return newStartDate;
        }
    }
}