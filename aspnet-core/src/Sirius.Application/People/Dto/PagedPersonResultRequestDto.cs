using Abp.Application.Services.Dto;

namespace Sirius.People.Dto
{
    public class PagedPersonResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public bool? IsActive { get; set; }
    }
}