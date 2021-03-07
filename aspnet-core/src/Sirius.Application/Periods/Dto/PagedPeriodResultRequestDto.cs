using System;
using Abp.Application.Services.Dto;
using Sirius.Shared.Enums;

namespace Sirius.Periods.Dto
{
    public class PagedPeriodResultRequestDto : PagedAndSortedResultRequestDto
    {
        public string Name { get; set; }
        public SiteOrBlock SiteOrBlock { get; set; }
        public Guid? BlockId { get; set; }
    }
}