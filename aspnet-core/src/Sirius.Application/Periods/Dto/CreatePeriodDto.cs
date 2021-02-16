using System;
using System.Collections.Generic;
using Sirius.Housings.Dto;

namespace Sirius.Periods.Dto
{
    public abstract class CreatePeriodDto
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public List<Guid> PaymentCategories { get; set; }
    }
}