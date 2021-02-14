using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Abp;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;

namespace Sirius.Periods
{
    [Table("AppPeriods")]
    public class Period : FullAuditedEntity<Guid>, IMustHaveTenant, IPassivable
    {
        protected Period()
        {
            
        }
        public int TenantId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; private set; }
        
        [Required] public DateTime StartDate { get; private set; }
        public bool IsActive { get;  set; }
        public DateTime? EndDate { get; private set; }

        public static Period Create(Guid id, int tenantId, [NotNull] string name, DateTime startDate)
        {
            return BindEntity(new Period(), id, tenantId, name, startDate, true);
        }

        public static Period Update(Period existingPeriod, [NotNull] string name)
        {
            return BindEntity(existingPeriod, existingPeriod.Id, existingPeriod.TenantId, name,
                existingPeriod.StartDate, existingPeriod.IsActive, existingPeriod.EndDate);
        }
        private static Period BindEntity(Period period, Guid id, int tenantId, string name, DateTime startDate, bool isActive,
            DateTime? endDate = null)
        {
            Check.NotNull(name, nameof(name));

            period ??= new Period();

            period.Id = id;
            period.TenantId = tenantId;

            period.Name = name;
            period.StartDate = startDate;
            period.IsActive = isActive;
            period.EndDate = endDate;

            return period;
        }

        public void ClosePeriod(Period period, DateTime endDate)
        {
            period.EndDate = endDate;
            period.IsActive = false;
        }
    }
}