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
        public string CategoryName { get; private set; }
        
        
        public static HousingCategory Create(Guid id, int tenantId, string categoryName)
        {
            return BindEntity(new HousingCategory(), id, tenantId, categoryName);
        }

        public static HousingCategory Update(HousingCategory existingHousingCategory, string categoryName)
        {
            return BindEntity(existingHousingCategory, existingHousingCategory.Id, existingHousingCategory.TenantId, categoryName);
        }

        private static HousingCategory BindEntity(HousingCategory housingCategory, Guid id, int tenantId, string categoryName)
        {
            housingCategory ??= new HousingCategory();

            housingCategory.Id = id;
            housingCategory.TenantId = tenantId;
            housingCategory.CategoryName = categoryName;

            return housingCategory;
        }
    }
}
