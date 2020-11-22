using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.UI;
using Sirius.People;

namespace Sirius.Housings
{
    public class HousingPersonPolicy : IHousingPersonPolicy
    {
        private readonly IRepository<HousingPerson, Guid> _housingPersonRepository;

        public HousingPersonPolicy(IRepository<HousingPerson, Guid> housingPersonRepository)
        {
            _housingPersonRepository = housingPersonRepository;
        }

        public async Task CheckAddPersonAttemptAsync(Housing housing, Person person)
        {
            if (housing == null)
            {
                throw new ArgumentNullException(nameof(Housing));
            }
            
            if (person == null)
            {
                throw new ArgumentNullException(nameof(Person));
            }

            var housingPeople =
                await _housingPersonRepository.GetAllListAsync(p =>
                     p.HousingId == housing.Id && p.PersonId == person.Id);

            if (housingPeople.Count > 0)
            {
                throw  new UserFriendlyException($"{person.FirstName} {person.LastName} bu konut için daha önceden tanımlanmış.");
            }
        }
    }
}