using System;
using Abp.AutoMapper;

namespace Sirius.Periods.Dto
{
    [AutoMapFrom(typeof(Period))]
    public class CreatePeriodDto
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
    }
}