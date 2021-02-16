using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Extensions;
using Abp.UI;
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

        public HousingDueType? HousingDueType { get; private set; }
        public bool IsValidForAllPeriods { get; private set; }
        public bool ShowInLists { get; private set; }
        public bool EditInAccountBook { get; private set; }
        public Guid? DefaultFromPaymentAccountId { get; private set; }
        public Guid? DefaultToPaymentAccountId { get; private set; }
        
        public bool IsActive { get; set; }

        public void SetPassive()
        {
            IsActive = false;
        }
        
        public static PaymentCategory Create(Guid id, int tenantId, string paymentCategoryName,
            HousingDueType? housingDueType, bool isValidForAllPeriods, Guid? defaultFromPaymentAccountId,
            Guid? defaultToPaymentAccountId, bool showInLists = true,
            bool editInAccountBook = true)
        {
            return BindEntity(new PaymentCategory(), id, tenantId, paymentCategoryName, housingDueType,
                isValidForAllPeriods, defaultFromPaymentAccountId, defaultToPaymentAccountId, showInLists,
                editInAccountBook, true);
        }

        public static PaymentCategory Update(PaymentCategory existingPaymentCategory, string paymentCategoryName,
            Guid? defaultFromPaymentAccountId, Guid? defaultToPaymentAccountId, bool showInLists = true)
        {
            return BindEntity(existingPaymentCategory, existingPaymentCategory.Id, existingPaymentCategory.TenantId,
                paymentCategoryName, existingPaymentCategory.HousingDueType,
                existingPaymentCategory.IsValidForAllPeriods, defaultFromPaymentAccountId, defaultToPaymentAccountId,
                showInLists, existingPaymentCategory.EditInAccountBook, true);
        }

        private static PaymentCategory BindEntity(PaymentCategory paymentCategory, Guid id, int tenantId,
            string paymentCategoryName, HousingDueType? housingDueType, bool isValidForAllPeriods, Guid?
                defaultFromPaymentAccountId, Guid? defaultToPaymentAccountId, bool showInLists,
            bool editInAccountBook, bool isActive)
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
            paymentCategory.HousingDueType = housingDueType;
            paymentCategory.IsValidForAllPeriods = isValidForAllPeriods;
            paymentCategory.ShowInLists = showInLists;
            paymentCategory.EditInAccountBook = editInAccountBook;
            paymentCategory.DefaultFromPaymentAccountId = defaultFromPaymentAccountId;
            paymentCategory.DefaultToPaymentAccountId = defaultToPaymentAccountId;
            paymentCategory.IsActive = isActive;

            return paymentCategory;
        }

    }
}