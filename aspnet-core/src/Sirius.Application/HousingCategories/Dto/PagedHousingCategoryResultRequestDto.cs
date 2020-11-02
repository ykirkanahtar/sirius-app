using Abp.Application.Services.Dto;

namespace Sirius.HousingCategories.Dto
{
    public class PagedHousingCategoryResultRequestDto : PagedAndSortedResultRequestDto
    {
        public string HousingCategoryName { get; set; }
    }
}