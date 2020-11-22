using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Repositories;
using Sirius.People;
using Sirius.Shared.Enums;

namespace Sirius.Housings
{
    [Table("AppHousingPeople")]
    public class HousingPerson : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        protected HousingPerson()
        {
            
        }
        public int TenantId { get; set; }

        public virtual Housing Housing { get; protected set; }
        public virtual Guid HousingId { get; protected set; }
        
        [ForeignKey("PersonId")]
        public virtual Person Person { get; protected set; }
        public virtual Guid PersonId { get; protected set; }

        public bool IsTenant { get; protected set; }
        public bool Contact { get; protected set; }
        
        public static async Task<HousingPerson> CreateAsync(Housing housing, Person person, bool isTenant, bool contact, IHousingPersonPolicy housingPersonPolicy)
        {
            await housingPersonPolicy.CheckAddPersonAttemptAsync(housing, person);

            return new HousingPerson()
            {
                TenantId = housing.TenantId,
                HousingId = housing.Id,
                Housing = housing,
                PersonId = person.Id,
                Person = person,
                IsTenant = isTenant,
                Contact = contact
            };
        }
    }
}