using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using Sirius.Employees.Dto;
using Sirius.Shared.Dtos;

namespace Sirius.Employees
{
    public class EmployeeAppService :
        AsyncCrudAppService<Employee, EmployeeDto, Guid, PagedEmployeeResultRequestDto, CreateEmployeeDto, EmployeeDto>,
        IEmployeeAppService
    {
        private readonly IEmployeeManager _employeeManager;
        private readonly IRepository<Employee, Guid> _employeeRepository;

        public EmployeeAppService(IEmployeeManager employeeManager, IRepository<Employee, Guid> employeeRepository)
            : base(employeeRepository)
        {
            _employeeManager = employeeManager;
            _employeeRepository = employeeRepository;
        }

        public override async Task<EmployeeDto> CreateAsync(CreateEmployeeDto input)
        {
            CheckCreatePermission();
            var employee = Employee.Create(SequentialGuidGenerator.Instance.Create(), AbpSession.GetTenantId(),
                input.FirstName, input.LastName, input.Phone1, input.Phone2);
            await _employeeManager.CreateAsync(employee);
            return ObjectMapper.Map<EmployeeDto>(employee);
        }

        public override async Task<EmployeeDto> UpdateAsync(EmployeeDto input)
        {
            CheckUpdatePermission();
            var existingEmployee = await _employeeManager.GetAsync(input.Id);
            var employee =
                Employee.Update(existingEmployee, input.FirstName, input.LastName, input.Phone1, input.Phone2);
            await _employeeManager.UpdateAsync(employee);
            return ObjectMapper.Map<EmployeeDto>(employee);
        }

        public override async Task DeleteAsync(EntityDto<Guid> input)
        {
            CheckDeletePermission();
            var employee = await _employeeManager.GetAsync(input.Id);
            await _employeeManager.DeleteAsync(employee);
        }

        public override async Task<PagedResultDto<EmployeeDto>> GetAllAsync(PagedEmployeeResultRequestDto input)
        {
            CheckGetAllPermission();
            var query = _employeeRepository.GetAll()
                .WhereIf(!string.IsNullOrWhiteSpace(input.Name),
                    p => (p.FirstName + " " + p.LastName).Contains(input.Name))
                .WhereIf(!string.IsNullOrWhiteSpace(input.PhoneNumber),
                    p => p.Phone1.Contains(input.PhoneNumber) ||
                         p.Phone2.Contains(input.PhoneNumber));

            var employees = await query.OrderBy(input.Sorting ?? $"{nameof(EmployeeDto.FirstName)} ASC, {nameof(EmployeeDto.LastName)}")
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<EmployeeDto>(query.Count(),
                ObjectMapper.Map<List<EmployeeDto>>(employees));
        }
        
        public async Task<List<LookUpDto>> GetEmployeeLookUpAsync()
        {
            CheckGetAllPermission();
            var employees = await _employeeRepository.GetAllListAsync();

            return
                (from l in employees
                    select new LookUpDto(l.Id.ToString(), $"{l.FirstName} {l.LastName}")).ToList();
        }


        public async Task<List<string>> GetEmployeeFromAutoCompleteFilterAsync(string request)
        {
            CheckGetAllPermission();
            var query = from p in _employeeRepository.GetAll()
                where p.FirstName.Contains(request) || p.LastName.Contains(request)
                select p.Name;

            return await query.ToListAsync();
        }
    }
}