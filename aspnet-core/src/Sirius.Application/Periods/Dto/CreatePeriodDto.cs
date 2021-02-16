using System;
using Abp.AutoMapper;
using Sirius.Housings.Dto;

namespace Sirius.Periods.Dto
{
    [AutoMapFrom(typeof(Period))]
    public class CreatePeriodForSiteDto
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
    }
}