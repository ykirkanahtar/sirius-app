using Abp.Application.Services.Dto;

namespace Sirius.Housings.Dto
{
    public class PagedHousingResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public bool? IsActive { get; set; }
    }
}