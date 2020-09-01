using Abp.Application.Services.Dto;

namespace Sirius.AccountBooks.Dto
{
    public class PagedAccountBookResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public bool? IsActive { get; set; }
    }
}