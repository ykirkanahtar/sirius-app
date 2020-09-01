using System;
using System.Threading.Tasks;
using Abp.Domain.Services;

namespace Sirius.People
{
    public interface IPersonManager : IDomainService
    {
        Task<Person> GetAsync(Guid id);
        Task CreateAsync(Person person);
        Task UpdateAsync(Person person);
        Task DeleteAsync(Person person);
    }
}
