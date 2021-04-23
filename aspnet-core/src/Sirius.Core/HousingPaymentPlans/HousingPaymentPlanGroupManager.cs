using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IRepository<HousingCategory, Guid> _housingCategoryRepository;
        private readonly IHousingPaymentPlanManager _housingPaymentPlanManager;
        private readonly IHousingManager _housingManager;
        private readonly IRepository<Housing, Guid> _housingRepository;

        public HousingPaymentPlanGroupManager(
            IRepository<HousingPaymentPlanGroup, Guid> housingPaymentPlanGroupRepository,
            IHousingManager housingManager,
            IRepository<HousingPaymentPlan, Guid> housingPaymentPlanRepository,
            IHousingPaymentPlanManager housingPaymentPlanManager
            , IRepository<HousingCategory, Guid> housingCategoryRepository,
            IRepository<Housing, Guid> housingRepository)
        {
            _housingPaymentPlanGroupRepository = housingPaymentPlanGroupRepository;
            _housingManager = housingManager;
            _housingPaymentPlanRepository = housingPaymentPlanRepository;
            _housingPaymentPlanManager = housingPaymentPlanManager;
            _housingCategoryRepository = housingCategoryRepository;
            _housingRepository = housingRepository;
        }

        private void CreateForHousing(HousingPaymentPlanGroup housingPaymentPlanGroup,
            List<HousingPaymentPlan> housingPaymentPlans, Housing housing, PaymentCategory paymentCategory,
            bool transferFromPreviousPeriod, DateTime? periodStartDate, decimal amountPerMonth, DateTime newStartDate)
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
                            , housingPaymentPlanGroup.ResidentOrOwner
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
                            , housingPaymentPlanGroup.ResidentOrOwner
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
                    , housingPaymentPlanGroup.ResidentOrOwner
                    , paymentCategory
                    , date
                    , amountPerMonth
                    , housingPaymentPlanGroup.Description
                    , HousingPaymentPlanType.HousingDueDefinition
                    , null
                    , null
                );

                housingPaymentPlans.Add(housingPaymentPlan);
            }
        }

        public async Task CreateAsync(HousingPaymentPlanGroup housingPaymentPlanGroup,
            DateTime startDate, PaymentCategory paymentCategory, bool transferFromPreviousPeriod,
            DateTime? periodStartDate)
        {
            var newStartDate = GetStartDate(startDate, housingPaymentPlanGroup.PaymentDayOfMonth);

            await _housingPaymentPlanGroupRepository.InsertAsync(housingPaymentPlanGroup);

            var housingPaymentPlans = new List<HousingPaymentPlan>();

            foreach (var housingPaymentPlanGroupHousingCategory in housingPaymentPlanGroup
                .HousingPaymentPlanGroupHousingCategories.Where(p => p.AmountPerMonth != 0))
            {
                var housingCategory =
                    await _housingCategoryRepository.GetAsync(housingPaymentPlanGroupHousingCategory.HousingCategoryId);
                var housings =
                    await _housingRepository.GetAllListAsync(p => p.HousingCategoryId == housingCategory.Id);

                foreach (var housing in housings)
                {
                    CreateForHousing(housingPaymentPlanGroup, housingPaymentPlans, housing, paymentCategory,
                        transferFromPreviousPeriod, periodStartDate,
                        housingPaymentPlanGroupHousingCategory.AmountPerMonth, newStartDate);
                }

                _housingManager.BulkIncreaseBalance(housings,
                    housingPaymentPlanGroupHousingCategory.AmountPerMonth * housingPaymentPlanGroup.CountOfMonth,
                    housingPaymentPlanGroup.ResidentOrOwner);
            }

            foreach (var housingPaymentPlanGroupHousing in housingPaymentPlanGroup
                .HousingPaymentPlanGroupHousings.Where(p => p.AmountPerMonth != 0))
            {
                var housing = await _housingRepository.GetAsync(housingPaymentPlanGroupHousing.HousingId);

                CreateForHousing(housingPaymentPlanGroup, housingPaymentPlans, housing, paymentCategory,
                    transferFromPreviousPeriod, periodStartDate,
                    housingPaymentPlanGroupHousing.AmountPerMonth, newStartDate);

                await _housingManager.IncreaseBalance(housing,
                    housingPaymentPlanGroupHousing.AmountPerMonth * housingPaymentPlanGroup.CountOfMonth,
                    housingPaymentPlanGroup.ResidentOrOwner);
            }

            await _housingPaymentPlanManager.BulkCreateAsync(housingPaymentPlans);
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

        private DateTime GetValidDate(int year, int month, int day)
        {
            try
            {
                return new DateTime(year, month, day);
            }
            catch
            {
                return new DateTime(year, month + 1, 1);
            }
        }

        public DateTime GetStartDate(DateTime startDate, int paymentDayOfMonth)
        {
            var firstPaymentDate = GetValidDate(startDate.Year, startDate.Month, paymentDayOfMonth);
            if (startDate > firstPaymentDate)
            {
                var year = startDate.Year;
                var month = startDate.Month == 12 ? 1 : startDate.Month + 1;
                year = month == 1 ? year + 1 : year;
                firstPaymentDate = new DateTime(year, month, paymentDayOfMonth);
            }

            return firstPaymentDate;
        }
    }
}