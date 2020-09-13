using Abp.Application.Services.Dto;

namespace Sirius.HousingCategories.Dto
{
    public class PagedHousingCategoryResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public bool? IsActive { get; set; }
    }
}