using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using JetBrains.Annotations;
using Sirius.Housings;
using Sirius.PaymentAccounts;
using Sirius.PaymentCategories;
using Sirius.Shared.Enums;

namespace Sirius.HousingPaymentPlans
{
    [Table("AppHousingPaymentPlanGroups")]
    public class HousingPaymentPlanGroup : AggregateRoot<Guid>, IFullAudited, IMustHaveTenant
    {
        protected HousingPaymentPlanGroup()
        {
            HousingPaymentPlanGroupHousingCategories = new List<HousingPaymentPlanGroupHousingCategory>();
        }

        public int TenantId { get; set; }

        public string HousingPaymentPlanGroupName { get; private set; }
        public Guid PaymentCategoryId { get; private set; }
        public decimal AmountPerMonth { get; private set; }
        public int CountOfMonth { get; private set; }
        public int PaymentDayOfMonth { get; set; }
        public DateTime StartDate { get; set; }
        public string Description { get; private set; }

        public ResidentOrOwner ResidentOrOwner { get; private set; }

        public virtual ICollection<HousingPaymentPlanGroupHousingCategory> HousingPaymentPlanGroupHousingCategories
        {
            get;
            private set;
        }

        [ForeignKey(nameof(PaymentCategoryId))]
        public virtual PaymentCategory PaymentCategory { get; protected set; }

        [ForeignKey(nameof(HousingPaymentPlan.HousingPaymentPlanGroupId))]
        public virtual ICollection<HousingPaymentPlan> HousingPaymentPlans { get; set; }

        public static HousingPaymentPlanGroup Create(Guid id,
            int tenantId, [NotNull] string housingPaymentPlanGroupName, 
            PaymentCategory paymentCategory, decimal amountPerMonth, int countOfMonth, int paymentDayOfMonth,
            DateTime startDate, string description, ResidentOrOwner residentOrOwner,
            List<HousingCategory> housingCategories)
        {
            var housingPaymentPlanGroupHousingCategories = new List<HousingPaymentPlanGroupHousingCategory>();
            housingCategories.ForEach(p =>
            {
                housingPaymentPlanGroupHousingCategories.Add(
                    HousingPaymentPlanGroupHousingCategory.Create(id, p.Id));
            });
            return BindEntity(new HousingPaymentPlanGroup(), id, tenantId,
                housingPaymentPlanGroupName, paymentCategory.Id, amountPerMonth, countOfMonth, paymentDayOfMonth, startDate,
                description, residentOrOwner, housingPaymentPlanGroupHousingCategories);
        }

        public static HousingPaymentPlanGroup Update(HousingPaymentPlanGroup existingHousingPaymentPlanGroup,
            [NotNull] string housingPaymentPlanGroupName)
        {
            return BindEntity(existingHousingPaymentPlanGroup,
                existingHousingPaymentPlanGroup.Id,
                existingHousingPaymentPlanGroup.TenantId,
                housingPaymentPlanGroupName,
                existingHousingPaymentPlanGroup.PaymentCategoryId,
                existingHousingPaymentPlanGroup.AmountPerMonth,
                existingHousingPaymentPlanGroup.CountOfMonth,
                existingHousingPaymentPlanGroup.PaymentDayOfMonth,
                existingHousingPaymentPlanGroup.StartDate,
                existingHousingPaymentPlanGroup.Description,
                existingHousingPaymentPlanGroup.ResidentOrOwner,
                existingHousingPaymentPlanGroup.HousingPaymentPlanGroupHousingCategories.ToList());
        }

        private static HousingPaymentPlanGroup BindEntity(HousingPaymentPlanGroup housingPaymentPlanGroup, Guid id,
            int tenantId, [NotNull] string housingPaymentPlanGroupName, 
            Guid paymentCategoryId, decimal amountPerMonth, int countOfMonth, int paymentDayOfMonth,
            DateTime startDate, string description, ResidentOrOwner residentOrOwner,
            List<HousingPaymentPlanGroupHousingCategory> housingPaymentPlanGroupHousingCategories)
        {
            housingPaymentPlanGroup ??= new HousingPaymentPlanGroup();

            housingPaymentPlanGroup.Id = id;
            housingPaymentPlanGroup.TenantId = tenantId;
            housingPaymentPlanGroup.HousingPaymentPlanGroupName = housingPaymentPlanGroupName;
            housingPaymentPlanGroup.PaymentCategoryId = paymentCategoryId;
            housingPaymentPlanGroup.AmountPerMonth = amountPerMonth;
            housingPaymentPlanGroup.CountOfMonth = countOfMonth;
            housingPaymentPlanGroup.PaymentDayOfMonth = paymentDayOfMonth;
            housingPaymentPlanGroup.StartDate = startDate;
            housingPaymentPlanGroup.Description = description;
            housingPaymentPlanGroup.ResidentOrOwner = residentOrOwner;
            housingPaymentPlanGroup.HousingPaymentPlanGroupHousingCategories = housingPaymentPlanGroupHousingCategories;
            housingPaymentPlanGroup.HousingPaymentPlans = new List<HousingPaymentPlan>();

            return housingPaymentPlanGroup;
        }

        public DateTime CreationTime { get; set; }
        public long? CreatorUserId { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public long? LastModifierUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletionTime { get; set; }
        public long? DeleterUserId { get; set; }
    }
}