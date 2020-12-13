using System;
using System.Threading.Tasks;
using Abp.Domain.Services;

namespace Sirius.AccountBooks
{
    public interface IAccountBookManager : IDomainService
    {
        Task CreateAsync(AccountBook accountBook);
        Task UpdateAsync(AccountBook accountBook);
        Task DeleteAsync(AccountBook accountBook);
        Task<AccountBook> GetAsync(Guid id);
        Task<AccountBookFile> GetAccountBookFileByUrlAsync(string url);
    }
}
