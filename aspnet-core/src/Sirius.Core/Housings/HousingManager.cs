using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore.Repositories;
using Abp.UI;
using Sirius.People;
using Sirius.Shared.Enums;

namespace Sirius.Housings
{
    public class HousingManager : IHousingManager
    {
        private readonly IRepository<Housing, Guid> _housingRepository;
        private readonly IRepository<HousingPerson> _housingPersonRepository;
        private readonly IHousingPersonPolicy _housingPersonPolicy;

        public HousingManager(IRepository<Housing, Guid> housingRepository, IRepository<HousingPerson> housingPersonRepository, IHousingPersonPolicy housingPersonPolicy)
        {
            _housingRepository = housingRepository;
            _housingPersonRepository = housingPersonRepository;
            _housingPersonPolicy = housingPersonPolicy;
        }

        public async Task CreateAsync(Housing housing)
        {
            await _housingRepository.InsertAsync(housing);
        }

        public async Task UpdateAsync(Housing housing)
        {
            await _housingRepository.UpdateAsync(housing);
        }

        public async Task DeleteAsync(Housing housing)
        {
            await _housingRepository.DeleteAsync(housing);
        }

        public async Task IncreaseBalance(Housing housing, decimal amount)
        {
            housing = Housing.IncreaseBalance(housing, amount);
            await _housingRepository.UpdateAsync(housing);
        }

        public void BulkIncreaseBalance(IEnumerable<Housing> housings, decimal amount)
        {
            var updatedHousings = housings.Select(housing => Housing.IncreaseBalance(housing, amount)).ToList();
            _housingRepository.GetDbContext().UpdateRange(updatedHousings);
        }

        public async Task DecreaseBalance(Housing housing, decimal amount)
        {
            housing = Housing.DecreaseBalance(housing, amount);
            await _housingRepository.UpdateAsync(housing);
        }

        public async Task<Housing> GetAsync(Guid id)
        {
            var housing = await _housingRepository.GetAsync(id);
            if (housing == null)
            {
                throw new UserFriendlyException("Konut bulunamadı");
            }

            return housing;
        }
        
        public async Task<HousingPerson> AddPersonAsync(Housing housing, Person person, HousingPersonType housingPersonType, bool contact)
        {
            return await _housingPersonRepository.InsertAsync(
                await HousingPerson.CreateAsync(housing, person, housingPersonType, contact,_housingPersonPolicy)
            );
        }
    }
}