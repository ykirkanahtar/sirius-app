using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Extensions;
using Abp.UI;
using JetBrains.Annotations;

namespace Sirius.Employees
{
    [Table("AppEmployees")]
    public class Employee : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        protected Employee()
        {

        }

        public int TenantId { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; protected set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; protected set; }

        [StringLength(50)]
        public string Phone1 { get; protected set; }

        [StringLength(50)]
        public string Phone2 { get; protected set; }

        public static Employee Create(Guid id, int tenantId, [NotNull] string firstName,
            [NotNull] string lastName, string phoneNumber1, string phoneNumber2)
        {
            return BindEntity(new Employee(), id, tenantId, firstName, lastName, phoneNumber1, phoneNumber2);
        }
        
        public static Employee Update(Employee existingEmployee, [NotNull] string firstName, [NotNull] string lastName, string phoneNumber1, string phoneNumber2)
        {
            return BindEntity(existingEmployee, existingEmployee.Id, existingEmployee.TenantId, firstName, lastName,
                phoneNumber1, phoneNumber2);
        }

        private static Employee BindEntity(Employee employee, Guid id, int tenantId, [NotNull] string firstName,
            [NotNull] string lastName, string phoneNumber1, string phoneNumber2)
        {
            Check.NotNull(firstName, nameof(firstName));
            Check.NotNull(lastName, nameof(lastName));

            employee ??= new Employee();
            
            employee.Id = id;
            employee.TenantId = tenantId;

            employee.SetName(firstName, lastName);
            employee.SetPhoneNumber1(phoneNumber1);
            employee.SetPhoneNumber2(phoneNumber2);

            return employee;
        }
        
        private void SetName([NotNull] string firstName, [NotNull] string lastName)
        {
            Check.NotNull(firstName, nameof(firstName));
            Check.NotNull(lastName, nameof(lastName));
            
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
