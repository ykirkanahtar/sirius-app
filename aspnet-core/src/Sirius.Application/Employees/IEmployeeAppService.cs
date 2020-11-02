using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Sirius.Employees.Dto;
using Sirius.Shared.Dtos;

namespace Sirius.Employees
{
    public interface IEmployeeAppService : IAsyncCrudAppService<EmployeeDto, Guid, PagedEmployeeResultRequestDto, CreateEmployeeDto, EmployeeDto>
    {
        Task<List<LookUpDto>> GetEmployeeLookUpAsync();
        Task<List<string>> GetEmployeeFromAutoCompleteFilterAsync(string request);
    }
}
