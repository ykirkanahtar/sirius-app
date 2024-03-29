using System;
using Abp.AutoMapper;

namespace Sirius.Periods.Dto
{
    [AutoMapFrom(typeof(Period))]
    public class CreatePeriodForBlockDto : CreatePeriodDto
    {
        public Guid BlockId { get; set; }
    }
}