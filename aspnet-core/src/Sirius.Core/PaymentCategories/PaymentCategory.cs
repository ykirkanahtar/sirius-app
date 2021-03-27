using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Extensions;
using Abp.UI;
using Sirius.PaymentAccounts;
using Sirius.Shared.Enums;

namespace Sirius.PaymentCategories
{
    [Table("AppPaymentCategories")]
    public class PaymentCategory : FullAuditedEntity<Guid>, IMustHaveTenant, IPassivable
    {
        protected PaymentCategory()
        {
        }

        public virtual int TenantId { get; set; }

        [StringLength(50)] public string PaymentCategoryName { get; private set; }
        public bool IsHousingDue { get; set; }
        public ResidentOrOwner? HousingDueForResidentOrOwner { get; set; }
        public bool IsValidForAllPeriods { get; private set; }
        public Guid? DefaultFromPaymentAccountId { get; private set; }
        public Guid? DefaultToPaymentAccountId { get; private set; }
        public PaymentCategoryType PaymentCategoryType { get; private set; }
        public bool IsActive { get; set; }

        public void SetPassive()
        {
            IsActive = false;
        }

        public static PaymentCategory CreateIncome(Guid id, int tenantId, string paymentCategoryName,
            /*HousingDueType? housingDueType,*/ bool isValidForAllPeriods,
            Guid defaultToPaymentAccountId)
        {
            return BindEntity(new PaymentCategory(), id, tenantId, paymentCategoryName,
                false, /*housingDueType,*/
                isValidForAllPeriods, null, defaultToPaymentAccountId, PaymentCategoryType.Income, true);
        }

        public static PaymentCategory CreateExpense(Guid id, int tenantId, string paymentCategoryName,
            bool isValidForAllPeriods, Guid defaultFromPaymentAccountId)
        {
            return BindEntity(new PaymentCategory(), id, tenantId, paymentCategoryName, false, /*null,*/
                isValidForAllPeriods, defaultFromPaymentAccountId, null, PaymentCategoryType.Expense, true);
        }

        public static PaymentCategory CreateHousingDue(Guid id, int tenantId, string paymentCategoryName,
            /*HousingDueType? housingDueType, */
            Guid defaultToPaymentAccountId, ResidentOrOwner housingDueForResidentOrOwner)
        {
            return BindEntity(new PaymentCategory(), id, tenantId, paymentCategoryName, true, /*housingDueType,*/
                false, null, defaultToPaymentAccountId, PaymentCategoryType.Income, true, housingDueForResidentOrOwner);
        }

        public static PaymentCategory CreateTransferBetweenAccounts(Guid id, int tenantId, string paymentCategoryName,
            /*HousingDueType? housingDueType, */bool isValidForAllPeriods, Guid defaultFromPaymentAccountId,
            Guid defaultToPaymentAccountId)
        {
            return BindEntity(new PaymentCategory(), id, tenantId, paymentCategoryName, false, /*housingDueType,*/
                isValidForAllPeriods, defaultFromPaymentAccountId, defaultToPaymentAccountId,
                PaymentCategoryType.TransferBetweenAccounts, true);
        }

        public static PaymentCategory Update(PaymentCategory existingPaymentCategory, string paymentCategoryName,
            Guid? defaultFromPaymentAccountId, Guid? defaultToPaymentAccountId)
        {
            return BindEntity(existingPaymentCategory, existingPaymentCategory.Id, existingPaymentCategory.TenantId,
                paymentCategoryName, existingPaymentCategory.IsHousingDue, /*existingPaymentCategory.HousingDueType,*/
                existingPaymentCategory.IsValidForAllPeriods, defaultFromPaymentAccountId, defaultToPaymentAccountId,
                existingPaymentCategory.PaymentCategoryType, true, existingPaymentCategory.HousingDueForResidentOrOwner);
        }

        public void SetDefaultFromPaymentAccount(PaymentAccount defaultFromPaymentAccount)
        {
            DefaultToPaymentAccountId = defaultFromPaymentAccount.Id;
        }

        public void SetDefaultToPaymentAccount(PaymentAccount defaultToPaymentAccount)
        {
            DefaultToPaymentAccountId = defaultToPaymentAccount.Id;
        }

        private static PaymentCategory BindEntity(PaymentCategory paymentCategory, Guid id, int tenantId,
            string paymentCategoryName, bool isHousingDue, /*HousingDueType? housingDueType, */
            bool isValidForAllPeriods, Guid?
                defaultFromPaymentAccountId, Guid? defaultToPaymentAccountId, PaymentCategoryType paymentCategoryType,
            bool isActive, ResidentOrOwner? housingDueForResidentOrOwner = null)
        {
            if (paymentCategoryName.IsNullOrWhiteSpace())
            {
                throw new UserFriendlyException("Kategori ismi dolu olmalıdır.");
            }

            if ((defaultFromPaymentAccountId.HasValue && defaultToPaymentAccountId.HasValue) &&
                (defaultFromPaymentAccountId.Value == defaultToPaymentAccountId.Value))
            {
                throw new UserFriendlyException("Varsayılan gelen hesap ile giden hesap aynı olamaz.");
            }

            paymentCategory ??= new PaymentCategory();

            paymentCategory.Id = id;
            paymentCategory.TenantId = tenantId;
            paymentCategory.PaymentCategoryName = paymentCategoryName;
            paymentCategory.IsHousingDue = isHousingDue;
            paymentCategory.IsValidForAllPeriods = isValidForAllPeriods;
            paymentCategory.DefaultFromPaymentAccountId = defaultFromPaymentAccountId;
            paymentCategory.DefaultToPaymentAccountId = defaultToPaymentAccountId;
            paymentCategory.IsActive = isActive;
            paymentCategory.PaymentCategoryType = paymentCategoryType;
            paymentCategory.HousingDueForResidentOrOwner = housingDueForResidentOrOwner;

            return paymentCategory;
        }
    }
}