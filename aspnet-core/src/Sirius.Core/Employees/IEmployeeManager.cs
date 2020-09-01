using Abp.Domain.Services;
using System;
using System.Threading.Tasks;
using Sirius.Employees;

namespace Sirius.Employees
{
    public interface IEmployeeManager : IDomainService
    {
        Task<Employee> GetAsync(Guid id);
        Task CreateAsync(Employee employee);
        Task UpdateAsync(Employee employee);
        Task DeleteAsync(Employee employee);
    }
}
