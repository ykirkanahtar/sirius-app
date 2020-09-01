using Abp.Application.Services.Dto;

namespace Sirius.Employees.Dto
{
    public class PagedEmployeeResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public bool? IsActive { get; set; }
    }
}