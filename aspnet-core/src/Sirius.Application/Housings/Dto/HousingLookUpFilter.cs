using System;

namespace Sirius.Housings.Dto
{
    public class HousingLookUpFilter
    {
        public Guid? PersonId { get; set; }
        public Guid? PaymentCategoryId { get; set; }
    }
}