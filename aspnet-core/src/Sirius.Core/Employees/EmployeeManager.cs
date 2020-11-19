using Abp.Domain.Repositories;
using Abp.UI;
using System;
using System.Threading.Tasks;
using Sirius.AppPaymentAccounts;

namespace Sirius.Employees
{
    public class EmployeeManager : IEmployeeManager
    {
        private readonly IRepository<Employee, Guid> _employeeRepository;
        private readonly IRepository<PaymentAccount, Guid> _paymentAccountRepository;

        public EmployeeManager(IRepository<Employee, Guid> employeeRepository, IRepository<PaymentAccount, Guid> paymentAccountRepository)
        {
            _employeeRepository = employeeRepository;
            _paymentAccountRepository = paymentAccountRepository;
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
            var paymentAccounts = await _paymentAccountRepository.GetAllListAsync(p => p.EmployeeId == employee.Id);
            if (paymentAccounts.Count > 0)
            {
                if (paymentAccounts.Count == 1)
                {
                    throw new UserFriendlyException($"Bu çalışana ait {paymentAccounts[0].AccountName} hesabı tanımlıdır. Silmek için önce tanımı kaldırınız.");
                }
                else
                {
                    throw new UserFriendlyException("Bu çalışan için birden fazla hesap tanımlıdır. Silmek için önce tanımları kaldırınız.");
                }
            }

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
