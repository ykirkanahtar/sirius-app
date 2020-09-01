using System;
using Abp.Application.Services;
using Sirius.Employees.Dto;

namespace Sirius.Employees
{
    public interface IEmployeeAppService : IAsyncCrudAppService<EmployeeDto, Guid, PagedEmployeeResultRequestDto, CreateEmployeeDto, EmployeeDto>
    {

    }
}
