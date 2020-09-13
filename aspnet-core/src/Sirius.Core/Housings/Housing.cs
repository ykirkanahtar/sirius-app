using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Extensions;
using Abp.UI;
using Sirius.HousingCategories;

namespace Sirius.Housings
{
    [Table("AppHousings")]
    public class Housing : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        protected Housing()
        {

        }

        public virtual int TenantId { get; set; }

        [StringLength(50)]
        public string Block { get; private set; }

        [StringLength(50)]
        public string Apartment { get; private set; }
        public Guid HousingCategoryId { get; private set; }
        
        
        [ForeignKey(nameof(HousingCategoryId))]
        public virtual  HousingCategory HousingCategory { get; set; }

        public string GetName()
        {
            if (!string.IsNullOrWhiteSpace(Block) && !string.IsNullOrWhiteSpace(Apartment))
                return $"{Block} - {Apartment}";

            if (string.IsNullOrWhiteSpace(Block))
                return Apartment;

            if (string.IsNullOrWhiteSpace(Apartment))
                return Block;

            return string.Empty;
        }
        
        public static Housing Create(Guid id, int tenantId, string block, string apartment, HousingCategory housingCategory)
        {
            return BindEntity(new Housing(), id, tenantId, block, apartment, housingCategory);
        }

        public static Housing Update(Housing existingHousing, string block, string apartment, HousingCategory housingCategory)
        {
            return BindEntity(existingHousing, existingHousing.Id, existingHousing.TenantId, block, apartment, housingCategory);
        }

        private static Housing BindEntity(Housing housing, Guid id, int tenantId, string block, string apartment, HousingCategory housingCategory)
        {
            if (block.IsNullOrWhiteSpace() && apartment.IsNullOrWhiteSpace())
            {
                throw new UserFriendlyException("Blok ve daire alanlarından en az biri dolu olmalıdır.");
            }
            
            housing ??= new Housing();

            housing.Id = id;
            housing.TenantId = tenantId;
            housing.Block = block;
            housing.Apartment = apartment;
            housing.HousingCategoryId = housingCategory.Id;

            return housing;
        }
    }
}
