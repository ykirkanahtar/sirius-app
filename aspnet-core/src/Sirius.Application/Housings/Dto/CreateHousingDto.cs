using System;
using Abp.AutoMapper;
using Sirius.HousingCategories;

namespace Sirius.Housings.Dto
{
    [AutoMapTo(typeof(Housing))]

    public class CreateHousingDto
    {
        public Guid? BlockId { get; set; }
        public string Apartment { get; set; }
        public Guid HousingCategoryId { get; set; }
    }
}
