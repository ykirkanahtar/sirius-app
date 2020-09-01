using System;
using System.Threading.Tasks;
using Abp.Domain.Services;

namespace Sirius.AccountBooks
{
    public interface IAccountBookManager : IDomainService
    {
        Task<AccountBook> GetAsync(Guid id);
        Task CreateAsync(AccountBook accountBook);
        Task UpdateAsync(AccountBook accountBook);
    }
}
