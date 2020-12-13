using System;
using System.Threading.Tasks;
using Abp.Domain.Services;

namespace Sirius.Housings
{
    public interface IHousingCategoryManager : IDomainService
    {
        Task<HousingCategory> GetAsync(Guid id);
        Task CreateAsync(HousingCategory housingCategory);
        Task UpdateAsync(HousingCategory housingCategory);
        Task DeleteAsync(HousingCategory housingCategory);
    }
}
