using Abp.Application.Services.Dto;

namespace Sirius.Housings.Dto
{
    public class PagedHousingCategoryResultRequestDto : PagedAndSortedResultRequestDto
    {
        public string HousingCategoryName { get; set; }
    }
}