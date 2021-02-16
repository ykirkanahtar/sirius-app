using System;
using Abp.AutoMapper;

namespace Sirius.Periods.Dto
{
    [AutoMapFrom(typeof(Period))]
    public class CreatePeriodForBlockDto
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public Guid BlockId { get; set; }
    }
}