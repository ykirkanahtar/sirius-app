using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.UI;

namespace Sirius.People
{
    public class PersonManager : IPersonManager
    {
        private readonly IRepository<Person, Guid> _personRepository;

        public PersonManager(IRepository<Person, Guid> personRepository)
        {
            _personRepository = personRepository;
        }

        public async Task CreateAsync(Person person)
        {
            await _personRepository.InsertAsync(person);
        }

        public async Task UpdateAsync(Person person)
        {
            await _personRepository.UpdateAsync(person);
        }
        
        public async Task DeleteAsync(Person person)
        {
            await _personRepository.DeleteAsync(person);
        }
        public async Task<Person> GetAsync(Guid id)
        {
            var person = await _personRepository.GetAsync(id);
            if(person == null)
            {
                throw new UserFriendlyException("Kişi bulunamadı");
            }
            return person;
        }
    }
}
