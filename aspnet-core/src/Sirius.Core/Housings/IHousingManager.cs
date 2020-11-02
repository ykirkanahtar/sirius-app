using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Services;
using Sirius.People;
using Sirius.Shared.Enums;

namespace Sirius.Housings
{
    public interface IHousingManager : IDomainService
    {
        Task<Housing> GetAsync(Guid id);
        Task CreateAsync(Housing housing);
        Task UpdateAsync(Housing housing);
        Task DeleteAsync(Housing housing);
        void BulkIncreaseBalance(IEnumerable<Housing> housings, decimal amount);
        Task IncreaseBalance(Housing housing, decimal amount);
        Task DecreaseBalance(Housing housing, decimal amount);
        Task<HousingPerson> AddPersonAsync(Housing housing, Person person, bool isTenant, bool contact);
        Task<List<Housing>> GetHousingsFromPersonIds(List<Guid> personIds);
    }
}
