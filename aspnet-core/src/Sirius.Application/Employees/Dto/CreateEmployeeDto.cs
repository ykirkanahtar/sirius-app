using Abp.AutoMapper;

namespace Sirius.Employees.Dto
{
    [AutoMapTo(typeof(Employee))]
    public class CreateEmployeeDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
    }
}
