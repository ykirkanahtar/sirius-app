using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Extensions;
using Abp.UI;
using JetBrains.Annotations;
using Sirius.Employees;

namespace Sirius.People
{
    [Table("AppPeople")]
    public class Person : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        protected Person()
        {

        }

        public int TenantId { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; private set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; private set; }

        [StringLength(50)]
        public string  Phone1 { get; private set; }

        [StringLength(50)]
        public string Phone2 { get; private set; }

        [NotMapped] public string Name => FirstName + " " + LastName;

        public static Person Create(Guid id, int tenantId, [NotNull] string firstName, [NotNull] string lastName, string phoneNumber1, string phoneNumber2)
        {
            return BindEntity(new Person(), id, tenantId, firstName, lastName, phoneNumber1, phoneNumber2);
        }

        public static Person Update(Person existingPerson, [NotNull] string firstName, [NotNull] string lastName, string phoneNumber1,
            string phoneNumber2)
        {
            return BindEntity(existingPerson, existingPerson.Id, existingPerson.TenantId, firstName, lastName,
                phoneNumber1, phoneNumber2);        
        }
        
        private static Person BindEntity(Person person, Guid id, int tenantId, [NotNull] string firstName,
            [NotNull] string lastName, string phoneNumber1, string phoneNumber2)
        {
            Check.NotNull(firstName, nameof(firstName));
            Check.NotNull(lastName, nameof(lastName));

            person ??= new Person();
            
            person.Id = id;
            person.TenantId = tenantId;

            person.SetName(firstName, lastName);
            person.SetPhoneNumber1(phoneNumber1);
            person.SetPhoneNumber2(phoneNumber2);

            return person;
        }
        
        private void SetName(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        private void SetPhoneNumber1(string phoneNumber)
        {
            if (!phoneNumber.IsNullOrWhiteSpace() && Phone2 == phoneNumber)
            {
                throw new UserFriendlyException("Bu telefon numarası kayıtlıdır");
            }

            Phone1 = phoneNumber;
        }

        private void SetPhoneNumber2(string phoneNumber)
        {
            if (!phoneNumber.IsNullOrWhiteSpace() && Phone1 == phoneNumber)
            {
                throw new UserFriendlyException("Bu telefon numarası kayıtlıdır");
            }

            Phone2 = phoneNumber;
        }
    }
}
