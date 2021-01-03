using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sirius.Housings;
using Sirius.PaymentAccounts;

namespace Sirius.People
{
    public class PersonManager : IPersonManager
    {
        private readonly IRepository<Person, Guid> _personRepository;
        private readonly IRepository<Housing, Guid> _housingRepository;
        private readonly IRepository<HousingPerson, Guid> _housingPersonRepository;
        private readonly IRepository<PaymentAccount, Guid> _paymentAccountRepository;

        public PersonManager(IRepository<Person, Guid> personRepository, IRepository<HousingPerson, Guid> housingPersonRepository, IRepository<Housing, Guid> housingRepository, IRepository<PaymentAccount, Guid> paymentAccountRepository)
        {
            _personRepository = personRepository;
            _housingPersonRepository = housingPersonRepository;
            _housingRepository = housingRepository;
            _paymentAccountRepository = paymentAccountRepository;
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
            var housingPeople = await _housingPersonRepository.GetAllListAsync(p => p.PersonId == person.Id);
            if (housingPeople.Count > 0)
            {
                if (housingPeople.Count == 1)
                {
                    var housing = await _housingRepository.GetAll().Include(p => p.Block).Where(p => p.Id == housingPeople[0].HousingId).SingleAsync();
                    throw new UserFriendlyException($"Bu kişi {housing.GetName()} konutu için tanımlıdır. Silmek için önce tanımı kaldırınız.");
                }
                else
                {
                    throw new UserFriendlyException("Bu kişi birden fazla konut için tanımlıdır. Silmek için önce tanımları kaldırınız.");
                }
            }

            var paymentAccounts = await _paymentAccountRepository.GetAllListAsync(p => p.PersonId == person.Id);
            if (paymentAccounts.Count > 0)
            {
                if (paymentAccounts.Count == 1)
                {
                    throw new UserFriendlyException($"Bu kişiye ait {paymentAccounts[0].AccountName} hesabı tanımlıdır. Silmek için önce tanımı kaldırınız.");
                }
                else
                {
                    throw new UserFriendlyException("Bu kişi için birden fazla hesap tanımlıdır. Silmek için önce tanımları kaldırınız.");
                }
            }

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
