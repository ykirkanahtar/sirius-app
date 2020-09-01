using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace Sirius.Employees.Dto
{
    [AutoMapFrom(typeof(Employee))]
    public class EmployeeDto : FullAuditedEntityDto<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
    }
}
