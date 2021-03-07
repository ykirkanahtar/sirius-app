using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sirius.Shared.Enums;

namespace Sirius.Periods.Dto
{
    [AutoMapFrom(typeof(Period))]
    public class PeriodDto : FullAuditedEntityDto<Guid>
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public bool IsActive { get;  set; }
        public DateTime? EndDate { get; set; }
        public SiteOrBlock SiteOrBlock { get; set; }
        public Guid? BlockId { get; set; }
    }
}