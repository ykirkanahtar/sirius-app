using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using JetBrains.Annotations;
using Sirius.Housings;
using Sirius.PaymentCategories;

namespace Sirius.HousingPaymentPlans
{
    [Table("AppHousingPaymentPlanGroups")]
    public class HousingPaymentPlanGroup : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        protected HousingPaymentPlanGroup()
        {
            
        }

        public int TenantId { get; set; }
        public string HousingPaymentPlanGroupName { get; private set; }
        public Guid HousingCategoryId { get; set; }
        public Guid PaymentCategoryId { get; private set; }
        public decimal AmountPerMonth { get; private set; }
        public int CountOfMonth { get; private set; }
        public int PaymentDayOfMonth { get; set; }
        public DateTime StartDate { get; set; }
        public string Description { get; private set; }
        
        [ForeignKey(nameof(HousingCategoryId))]
        public virtual HousingCategory HousingCategory { get; protected set; }
        
        [ForeignKey(nameof(PaymentCategoryId))]
        public virtual PaymentCategory PaymentCategory { get; protected set; }
        
        [ForeignKey(nameof(HousingPaymentPlan.HousingPaymentPlanGroupId))] 
        public virtual ICollection<HousingPaymentPlan> HousingPaymentPlans { get; set; }

        public static HousingPaymentPlanGroup Create(Guid id,
            int tenantId, [NotNull] string housingPaymentPlanGroupName, HousingCategory housingCategory,
            PaymentCategory paymentCategory, decimal amountPerMonth, int countOfMonth, int paymentDayOfMonth,
            DateTime startDate, string description)
        {
            return BindEntity(new HousingPaymentPlanGroup(), id, tenantId, housingPaymentPlanGroupName,
                housingCategory.Id, paymentCategory.Id, amountPerMonth, countOfMonth, paymentDayOfMonth, startDate,
                description);
        }

        public static HousingPaymentPlanGroup Update(HousingPaymentPlanGroup existingHousingPaymentPlanGroup,
            [NotNull] string housingPaymentPlanGroupName)
        {
            return BindEntity(existingHousingPaymentPlanGroup, 
                existingHousingPaymentPlanGroup.Id, 
                existingHousingPaymentPlanGroup.TenantId, 
                housingPaymentPlanGroupName,
                existingHousingPaymentPlanGroup.HousingCategoryId, 
                existingHousingPaymentPlanGroup.PaymentCategoryId, 
                existingHousingPaymentPlanGroup.AmountPerMonth, 
                existingHousingPaymentPlanGroup.CountOfMonth, 
                existingHousingPaymentPlanGroup.PaymentDayOfMonth, 
                existingHousingPaymentPlanGroup.StartDate,
                existingHousingPaymentPlanGroup.Description);
        }

        private static HousingPaymentPlanGroup BindEntity(HousingPaymentPlanGroup housingPaymentPlanGroup, Guid id,
            int tenantId, [NotNull] string housingPaymentPlanGroupName, Guid housingCategoryId,
            Guid paymentCategoryId, decimal amountPerMonth, int countOfMonth, int paymentDayOfMonth,
            DateTime startDate, string description)
        {
            housingPaymentPlanGroup ??= new HousingPaymentPlanGroup();

            housingPaymentPlanGroup.Id = id;
            housingPaymentPlanGroup.TenantId = tenantId;
            housingPaymentPlanGroup.HousingPaymentPlanGroupName = housingPaymentPlanGroupName;
            housingPaymentPlanGroup.HousingCategoryId = housingCategoryId;
            housingPaymentPlanGroup.PaymentCategoryId = paymentCategoryId;
            housingPaymentPlanGroup.AmountPerMonth = amountPerMonth;
            housingPaymentPlanGroup.CountOfMonth = countOfMonth;
            housingPaymentPlanGroup.PaymentDayOfMonth = paymentDayOfMonth;
            housingPaymentPlanGroup.StartDate = startDate;
            housingPaymentPlanGroup.Description = description;
            housingPaymentPlanGroup.HousingPaymentPlans = new List<HousingPaymentPlan>();

            return housingPaymentPlanGroup;
        }
    }
}