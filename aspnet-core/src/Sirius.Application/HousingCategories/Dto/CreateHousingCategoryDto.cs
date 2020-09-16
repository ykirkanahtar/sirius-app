using Abp.AutoMapper;

namespace Sirius.HousingCategories.Dto
{
    [AutoMapTo(typeof(HousingCategory))]

    public class CreateHousingCategoryDto
    {
        public string HousingCategoryName { get; set; }
    }
}
