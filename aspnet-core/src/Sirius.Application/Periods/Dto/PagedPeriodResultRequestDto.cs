using System;
using Abp.Application.Services.Dto;

namespace Sirius.Periods.Dto
{
    public class PagedPeriodResultRequestDto : PagedAndSortedResultRequestDto
    {
        public string Name { get; set; }
    }
}