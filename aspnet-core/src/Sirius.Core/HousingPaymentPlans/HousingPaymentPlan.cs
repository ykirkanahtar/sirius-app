using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using JetBrains.Annotations;
using Sirius.Housings;
using Sirius.PaymentAccounts;
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
        public Guid? HousingPaymentPlanGroupId { get; private set; }
        public Guid HousingId { get; private set; }
        public Guid? PaymentCategoryId { get; private set; }
        public DateTime Date { get; private set; }
        public HousingPaymentPlanType HousingPaymentPlanType { get; private set; }
        public CreditOrDebt CreditOrDebt { get; private set; }
        public ResidentOrOwner ResidentOrOwner { get; private set; }
        public decimal Amount { get; private set; }
        public string Description { get; private set; }
        public Guid? AccountBookId { get; private set; }
        public Guid? TransferFromPaymentCategoryId { get; private set; }
        public ResidentOrOwner? FirstHousingDueTransferIsResidentOrOwner { get; private set; } 
        
        [ForeignKey(nameof(PaymentCategoryId))]
        public virtual PaymentCategory PaymentCategory { get; protected set; }

        [ForeignKey(nameof(TransferFromPaymentCategoryId))]
        public virtual PaymentCategory TransferFromPaymentCategory { get; protected set; }

        public static HousingPaymentPlan CreateDebt(Guid id, int tenantId,
            [CanBeNull] HousingPaymentPlanGroup housingPaymentPlanGroup, Housing housing, ResidentOrOwner residentOrOwner,
            [CanBeNull] PaymentCategory paymentCategory, DateTime date, decimal amount, string description,
            HousingPaymentPlanType housingPaymentPlanType,
            [CanBeNull] PaymentCategory transferFromPaymentCategory,
            ResidentOrOwner? firstHousingDueTransferIsResidentOrOwner) //tanımlanacak aidatlar
        {
            return BindEntity(new HousingPaymentPlan(), id, tenantId, housingPaymentPlanGroup?.Id,
                CreditOrDebt.Debt, housing.Id, residentOrOwner, paymentCategory?.Id, date, amount, description, null,
                housingPaymentPlanType, transferFromPaymentCategory?.Id, firstHousingDueTransferIsResidentOrOwner);
        }

        public static HousingPaymentPlan CreateCredit(Guid id, int tenantId, 
            Housing housing, ResidentOrOwner residentOrOwner, [CanBeNull] PaymentCategory paymentCategory, DateTime date, decimal amount,
            string description,
            [CanBeNull] AccountBook accountBook, HousingPaymentPlanType housingPaymentPlanType,
            [CanBeNull] PaymentCategory transferFromPaymentCategory,
            ResidentOrOwner? firstHousingDueTransferIsResidentOrOwner) //tahsil edilen aidatlar
        {
            return BindEntity(new HousingPaymentPlan(), id, tenantId, null,
                CreditOrDebt.Credit, housing.Id, residentOrOwner,
                paymentCategory?.Id, date, amount, description, accountBook?.Id, housingPaymentPlanType,
                transferFromPaymentCategory?.Id, firstHousingDueTransferIsResidentOrOwner);
        }

        internal static HousingPaymentPlan Update(HousingPaymentPlan existingHousingPaymentPlan, DateTime date,
            decimal amount, string description)
        {
            return BindEntity(existingHousingPaymentPlan, existingHousingPaymentPlan.Id,
                existingHousingPaymentPlan.TenantId, existingHousingPaymentPlan.HousingPaymentPlanGroupId,
                existingHousingPaymentPlan.CreditOrDebt, existingHousingPaymentPlan.HousingId, existingHousingPaymentPlan.ResidentOrOwner,
                existingHousingPaymentPlan.PaymentCategoryId, date, amount,
                description, existingHousingPaymentPlan.AccountBookId,
                existingHousingPaymentPlan.HousingPaymentPlanType,
                existingHousingPaymentPlan.TransferFromPaymentCategoryId, 
                existingHousingPaymentPlan.FirstHousingDueTransferIsResidentOrOwner);
        }

        public static HousingPaymentPlan UpdateForFirstHousingDueTransfer(HousingPaymentPlan existingHousingPaymentPlan, 
            ResidentOrOwner residentOrOwner, decimal amount, bool isDebt, DateTime date, string description)
        {
            return BindEntity(existingHousingPaymentPlan, existingHousingPaymentPlan.Id,
                existingHousingPaymentPlan.TenantId, existingHousingPaymentPlan.HousingPaymentPlanGroupId,
                isDebt  ? CreditOrDebt.Debt : CreditOrDebt.Credit, existingHousingPaymentPlan.HousingId, residentOrOwner,
                existingHousingPaymentPlan.PaymentCategoryId, date, amount,
                description, existingHousingPaymentPlan.AccountBookId,
                existingHousingPaymentPlan.HousingPaymentPlanType,
                existingHousingPaymentPlan.TransferFromPaymentCategoryId, 
                residentOrOwner);
        }

        private static HousingPaymentPlan BindEntity(HousingPaymentPlan housingPaymentPlan, Guid id, int tenantId,
            Guid? housingPaymentPlanGroupId, CreditOrDebt creditOrDebt, Guid housingId, ResidentOrOwner residentOrOwner, Guid? paymentCategoryId,
            DateTime date, decimal amount, string description, Guid? accountBookId,
            HousingPaymentPlanType housingPaymentPlanType, Guid? transferFromPaymentCategoryId, 
            ResidentOrOwner? firstHousingDueTransferIsResidentOrOwner)
        {
            housingPaymentPlan ??= new HousingPaymentPlan();

            housingPaymentPlan.Id = id;
            housingPaymentPlan.TenantId = tenantId;
            housingPaymentPlan.HousingPaymentPlanGroupId = housingPaymentPlanGroupId;
            housingPaymentPlan.HousingId = housingId;
            housingPaymentPlan.ResidentOrOwner = residentOrOwner;
            housingPaymentPlan.PaymentCategoryId = paymentCategoryId;
            housingPaymentPlan.CreditOrDebt = creditOrDebt;
            housingPaymentPlan.Date = date.Date + new TimeSpan(0, 0, 0);
            housingPaymentPlan.Amount = Math.Abs(amount);
            housingPaymentPlan.Description = description;
            housingPaymentPlan.AccountBookId = creditOrDebt == CreditOrDebt.Credit ? accountBookId : null;
            housingPaymentPlan.HousingPaymentPlanType = housingPaymentPlanType;
            housingPaymentPlan.TransferFromPaymentCategoryId = transferFromPaymentCategoryId;
            housingPaymentPlan.FirstHousingDueTransferIsResidentOrOwner = firstHousingDueTransferIsResidentOrOwner;
            
            return housingPaymentPlan;
        }

        public void SetAbsAmount()
        {
            Amount = Math.Abs(Amount);
        }
    }
}