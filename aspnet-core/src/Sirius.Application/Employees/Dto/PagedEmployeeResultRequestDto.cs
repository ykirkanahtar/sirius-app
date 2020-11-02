using Abp.Application.Services.Dto;

namespace Sirius.Employees.Dto
{
    public class PagedEmployeeResultRequestDto : PagedAndSortedResultRequestDto
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
    }
}