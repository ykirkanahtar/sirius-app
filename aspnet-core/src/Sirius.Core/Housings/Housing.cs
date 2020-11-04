using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Extensions;
using Abp.UI;
using JetBrains.Annotations;
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

        public Guid BlockId { get; private set; }

        [StringLength(50)] public string Apartment { get; private set; }
        public Guid HousingCategoryId { get; private set; }

        [DefaultValue(0)] public decimal Balance { get; private set; }

        [ForeignKey(nameof(HousingCategoryId))]
        public virtual HousingCategory HousingCategory { get; set; }

        [ForeignKey(nameof(BlockId))] public virtual Block Block { get; set; }

        [ForeignKey("HousingId")] public virtual ICollection<HousingPerson> HousingPeople { get; protected set; }

        public string GetName()
        {
            return $"{Block.BlockName} - {Apartment}";
        }

        public static async Task<Housing> CreateAsync(IHousingPolicy housingPolicy, Guid id, int tenantId, Block block, string apartment,
            HousingCategory housingCategory)
        {
            return await BindEntityAsync(housingPolicy, false, new Housing(), id, tenantId, block, apartment, housingCategory);
        }

        public static async Task<Housing> UpdateAsync(IHousingPolicy housingPolicy, Housing existingHousing, Block block,
            string apartment,
            HousingCategory housingCategory)
        {
            return await BindEntityAsync(housingPolicy, true, existingHousing, existingHousing.Id, existingHousing.TenantId, block,
                apartment,
                housingCategory);
        }

        private static async Task<Housing> BindEntityAsync(IHousingPolicy housingPolicy, bool isUpdate, Housing housing, Guid id,
            int tenantId, Block block, string apartment,
            HousingCategory housingCategory)
        {
            housing ??= new Housing();

            housing.Id = id;
            housing.TenantId = tenantId;
            housing.BlockId = block.Id;
            housing.Apartment = apartment;
            housing.HousingCategoryId = housingCategory.Id;
            housing.HousingPeople = new Collection<HousingPerson>();

            await housingPolicy.CheckCreateOrUpdateHousingAttemptAsync(housing, isUpdate);

            return housing;
        }

        public static Housing IncreaseBalance(Housing housing, decimal amount)
        {
            if (amount < 0)
            {
                throw new UserFriendlyException("Tutar sıfırdan küçük olamaz");
            }

            housing.Balance += amount;
            return housing;
        }

        public static Housing DecreaseBalance(Housing housing, decimal amount)
        {
            if (amount < 0)
            {
                throw new UserFriendlyException("Tutar sıfırdan küçük olamaz");
            }

            housing.Balance -= amount;
            return housing;
        }
    }
}