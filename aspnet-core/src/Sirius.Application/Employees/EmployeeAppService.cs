using System;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Sirius.Employees.Dto;

namespace Sirius.Employees
{
    public class EmployeeAppService : AsyncCrudAppService<Employee, EmployeeDto, Guid, PagedEmployeeResultRequestDto, CreateEmployeeDto, EmployeeDto>, IEmployeeAppService
    {
        private readonly IEmployeeManager _employeeManager;

        public EmployeeAppService(IEmployeeManager employeeManager, IRepository<Employee, Guid> employeeRepository) 
            : base(employeeRepository)
        {
            _employeeManager = employeeManager;
        }

        public override async Task<EmployeeDto> CreateAsync(CreateEmployeeDto input)
        {
            var employee = Employee.Create(SequentialGuidGenerator.Instance.Create(), AbpSession.GetTenantId(), input.FirstName, input.LastName, input.Phone1, input.Phone2);
            await _employeeManager.CreateAsync(employee);
            return ObjectMapper.Map<EmployeeDto>(employee);
        }
        
        public override async Task<EmployeeDto> UpdateAsync(EmployeeDto input)
        {
            var existingEmployee = await _employeeManager.GetAsync(input.Id);
            var employee =
                Employee.Update(existingEmployee, input.FirstName, input.LastName, input.Phone1, input.Phone2);
            await _employeeManager.UpdateAsync(employee);
            return ObjectMapper.Map<EmployeeDto>(employee);
        }

        public override async Task DeleteAsync(EntityDto<Guid> input)
        {
            var employee = await _employeeManager.GetAsync(input.Id);
            await _employeeManager.DeleteAsync(employee);
        }
    }
}
