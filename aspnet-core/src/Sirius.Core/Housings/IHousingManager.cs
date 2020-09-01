using System;
using System.Threading.Tasks;
using Abp.Domain.Services;

namespace Sirius.Housings
{
    public interface IHousingManager : IDomainService
    {
        Task<Housing> GetAsync(Guid id);
        Task CreateAsync(Housing housing);
        Task UpdateAsync(Housing housing);
        Task DeleteAsync(Housing housing);
    }
}
