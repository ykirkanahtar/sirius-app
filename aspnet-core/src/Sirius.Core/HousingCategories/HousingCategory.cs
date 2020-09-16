using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace Sirius.HousingCategories
{
    [Table("AppHousingCategories")]
    public class HousingCategory : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        protected HousingCategory()
        {

        }

        public virtual int TenantId { get; set; }

        [StringLength(50)]
        [Required]
        public string HousingCategoryName { get; private set; }
        
        
        public static HousingCategory Create(Guid id, int tenantId, string housingCategoryName)
        {
            return BindEntity(new HousingCategory(), id, tenantId, housingCategoryName);
        }

        public static HousingCategory Update(HousingCategory existingHousingCategory, string housingCategoryName)
        {
            return BindEntity(existingHousingCategory, existingHousingCategory.Id, existingHousingCategory.TenantId, housingCategoryName);
        }

        private static HousingCategory BindEntity(HousingCategory housingCategory, Guid id, int tenantId, string housingCategoryName)
        {
            housingCategory ??= new HousingCategory();

            housingCategory.Id = id;
            housingCategory.TenantId = tenantId;
            housingCategory.HousingCategoryName = housingCategoryName;

            return housingCategory;
        }
    }
}
