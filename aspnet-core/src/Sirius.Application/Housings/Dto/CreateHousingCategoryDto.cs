using Abp.AutoMapper;

namespace Sirius.Housings.Dto
{
    [AutoMapTo(typeof(HousingCategory))]

    public class CreateHousingCategoryDto
    {
        public string HousingCategoryName { get; set; }
    }
}
