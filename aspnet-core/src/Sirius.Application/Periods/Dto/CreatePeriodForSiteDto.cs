using System;
using System.Collections.Generic;
using Abp.AutoMapper;

namespace Sirius.Periods.Dto
{
    [AutoMapFrom(typeof(Period))]
    public class CreatePeriodForSiteDto : CreatePeriodDto
    {
        
    }
}