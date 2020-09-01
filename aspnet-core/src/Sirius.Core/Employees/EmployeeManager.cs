using Abp.Domain.Repositories;
using Abp.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Sirius.Employees;

namespace Sirius.Employees
{
    public class EmployeeManager : IEmployeeManager
    {
        private readonly IRepository<Employee, Guid> _employeeRepository;

        public EmployeeManager(IRepository<Employee, Guid> employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task CreateAsync(Employee employee)
        {
            await _employeeRepository.InsertAsync(employee);
        }

        public async Task UpdateAsync(Employee employee)
        {
            await _employeeRepository.UpdateAsync(employee);
        }
        
        public async Task DeleteAsync(Employee employee)
        {
            await _employeeRepository.DeleteAsync(employee);
        }
        
        public async Task<Employee> GetAsync(Guid id)
        {
            var employee = await _employeeRepository.GetAsync(id);
            if(employee == null)
            {
                throw new UserFriendlyException("Çalışan bulunamadı");
            }
            return employee;
        }
    }
}
