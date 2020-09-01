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
    public class PaymentCategory : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        protected PaymentCategory()
        {

        }

        public virtual int? TenantId { get; set; }

        [StringLength(50)]
        public string PaymentCategoryName { get; private set; }
        
        public HousingDueType? HousingDueType { get; private set; }
        public bool IsValidForAllPeriods { get; private set; }
        
        public static PaymentCategory Create(Guid id, int? tenantId, string paymentCategoryName, HousingDueType? housingDueType, bool isValidForAllPeriods)
        {
            return BindEntity(new PaymentCategory(), id, tenantId, paymentCategoryName, housingDueType, isValidForAllPeriods);
        }

        public static PaymentCategory Update(PaymentCategory existingPaymentCategory, string paymentCategoryName)
        {
            return BindEntity(existingPaymentCategory, existingPaymentCategory.Id, existingPaymentCategory.TenantId,
                paymentCategoryName, existingPaymentCategory.HousingDueType, existingPaymentCategory.IsValidForAllPeriods);
        }

        private static PaymentCategory BindEntity(PaymentCategory paymentCategory, Guid id, int? tenantId,
            string paymentCategoryName, HousingDueType? housingDueType, bool isValidForAllPeriods)
        {
            if (paymentCategoryName.IsNullOrWhiteSpace())
            {
                throw new UserFriendlyException("Kategori ismi dolu olmalıdır.");
            }

            paymentCategory ??= new PaymentCategory();

            paymentCategory.Id = id;
            paymentCategory.TenantId = tenantId;
            paymentCategory.PaymentCategoryName = paymentCategoryName;
            paymentCategory.HousingDueType = housingDueType;
            paymentCategory.IsValidForAllPeriods = isValidForAllPeriods;

            return paymentCategory;
        }
    }
}
