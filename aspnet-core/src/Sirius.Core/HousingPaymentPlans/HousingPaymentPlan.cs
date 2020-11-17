using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using JetBrains.Annotations;
using Sirius.AccountBooks;
using Sirius.Housings;
using Sirius.PaymentCategories;
using Sirius.Shared.Enums;

namespace Sirius.HousingPaymentPlans
{
    [Table("AppHousingPaymentPlans")]
    public class HousingPaymentPlan : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        protected HousingPaymentPlan()
        {
        }

        public virtual int TenantId { get; set; }
        public Guid HousingId { get; private set; }

        public Guid PaymentCategoryId { get; private set; }
        public DateTime Date { get; private set; }
        public PaymentPlanType PaymentPlanType { get; private set; }
        public decimal Amount { get; private set; }
        public string Description { get; private set; }
        public Guid? AccountBookId { get; private set; }

        [ForeignKey(nameof(PaymentCategoryId))]
        public virtual PaymentCategory PaymentCategory { get; set; }

        public static HousingPaymentPlan CreateDebt(Guid id, int tenantId, Housing housing,
            PaymentCategory paymentCategory, DateTime date, decimal amount, string description) //tanımlanacak aidatlar
        {
            return BindEntity(new HousingPaymentPlan(), id, tenantId, PaymentPlanType.Debt, housing.Id,
                paymentCategory.Id, date, amount, description, null);
        }

        public static HousingPaymentPlan CreateCredit(Guid id, int tenantId, Housing housing,
            PaymentCategory paymentCategory, DateTime date, decimal amount, string description,
            [CanBeNull] AccountBook accountBook) //tahsil edilen aidatlar
        {
            return BindEntity(new HousingPaymentPlan(), id, tenantId, PaymentPlanType.Credit, housing.Id,
                paymentCategory.Id, date, amount, description, accountBook?.Id);
        }

        public static HousingPaymentPlan Update(HousingPaymentPlan existingHousingPaymentPlan, DateTime date,
            decimal amount, string description)
        {
            return BindEntity(existingHousingPaymentPlan, existingHousingPaymentPlan.Id,
                existingHousingPaymentPlan.TenantId, existingHousingPaymentPlan.PaymentPlanType,
                existingHousingPaymentPlan.HousingId, existingHousingPaymentPlan.PaymentCategoryId, date, amount,
                description, null);
        }

        private static HousingPaymentPlan BindEntity(HousingPaymentPlan housingPaymentPlan, Guid id, int tenantId,
            PaymentPlanType paymentPlanType, Guid housingId, Guid paymentCategoryId, DateTime date, decimal amount,
            string description, Guid? accountBookId)
        {
            housingPaymentPlan ??= new HousingPaymentPlan();

            housingPaymentPlan.Id = id;
            housingPaymentPlan.TenantId = tenantId;
            housingPaymentPlan.HousingId = housingId;
            housingPaymentPlan.PaymentCategoryId = paymentCategoryId;
            housingPaymentPlan.PaymentPlanType = paymentPlanType;
            housingPaymentPlan.Date = date;
            housingPaymentPlan.Amount =
                paymentPlanType == PaymentPlanType.Debt ? Math.Abs(amount) : Math.Abs(amount) * -1;
            housingPaymentPlan.Description = description;
            housingPaymentPlan.AccountBookId = paymentPlanType == PaymentPlanType.Credit ? accountBookId : null;

            return housingPaymentPlan;
        }
    }
}