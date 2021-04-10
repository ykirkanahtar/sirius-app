using System;
using System.Collections.Generic;
using Sirius.HousingPaymentPlans.Dto;
using Sirius.Housings.Dto;

namespace Sirius.Periods.Dto
{
    public abstract class CreatePeriodDto
    {
        public CreatePeriodDto()
        {
            PaymentCategories = new List<Guid>();
        }
        
        public string Name { get; set; }
        public string StartDateString { get; set; }
        public string EndDateString { get; set; }
        
        public List<Guid> PaymentCategories { get; set; }
    }
}