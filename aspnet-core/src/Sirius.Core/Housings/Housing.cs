using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Extensions;
using Abp.UI;

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

        public static Housing Create(Guid id, int tenantId, string block, string apartment)
        {
            return BindEntity(new Housing(), id, tenantId, block, apartment);
        }

        public static Housing Update(Housing existingHousing, string block, string apartment)
        {
            return BindEntity(existingHousing, existingHousing.Id, existingHousing.TenantId, block, apartment);
        }

        private static Housing BindEntity(Housing housing, Guid id, int tenantId, string block, string apartment)
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

            return housing;
        }
    }
}
