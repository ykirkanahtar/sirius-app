using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Sirius.People;

namespace Sirius.Housings
{
    public interface IHousingPersonPolicy : IDomainService
    {
        Task CheckAddPersonAttemptAsync(Housing housing, Person person);
    }
}