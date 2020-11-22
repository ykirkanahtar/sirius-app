using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sirius.AccountBooks;
using Sirius.HousingPaymentPlans;
using Sirius.People;
using Sirius.Shared.Enums;

namespace Sirius.Housings
{
    public class HousingManager : IHousingManager
    {
        private readonly IRepository<Housing, Guid> _housingRepository;
        private readonly IRepository<HousingPerson, Guid> _housingPersonRepository;
        private readonly IRepository<AccountBook, Guid> _accountBookRepository;
        private readonly IRepository<HousingPaymentPlan, Guid> _housingPaymentPlanRepository;
        private readonly IHousingPersonPolicy _housingPersonPolicy;

        public HousingManager(IRepository<Housing, Guid> housingRepository, IRepository<HousingPerson, Guid> housingPersonRepository, IHousingPersonPolicy housingPersonPolicy, IRepository<AccountBook, Guid> accountBookRepository, IRepository<HousingPaymentPlan, Guid> housingPaymentPlanRepository)
        {
            _housingRepository = housingRepository;
            _housingPersonRepository = housingPersonRepository;
            _housingPersonPolicy = housingPersonPolicy;
            _accountBookRepository = accountBookRepository;
            _housingPaymentPlanRepository = housingPaymentPlanRepository;
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
            var housingPaymentPlans = await _housingPaymentPlanRepository.GetAllListAsync(p => p.HousingId == housing.Id);
            if(housingPaymentPlans.Count > 0)
            {
                throw new UserFriendlyException("Aidat ödeme planı oluşmuş bir konut silinemez.");
            }

            var housingAccountBooks = await _accountBookRepository.GetAllListAsync(p => p.HousingId == housing.Id);
            if(housingAccountBooks.Count > 0)
            {
                throw new UserFriendlyException("İşletme defteri kaydı olan bir konut silinemez.");
            }

            var housingPeople = await _housingPersonRepository.GetAllListAsync(p => p.HousingId == housing.Id);
            foreach (var housingPerson in housingPeople)
            {
                await _housingPersonRepository.DeleteAsync(housingPerson);
            }
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
        
        public async Task<HousingPerson> AddPersonAsync(Housing housing, Person person, bool isTenant, bool contact)
        {
            return await _housingPersonRepository.InsertAsync(
                await HousingPerson.CreateAsync(housing, person, isTenant, contact,_housingPersonPolicy)
            );
        }

        public async Task<List<Housing>> GetHousingsFromPersonIds(List<Guid> personIds)
        {
            var query = _housingPersonRepository.GetAll().Where(p => personIds.Contains(p.PersonId))
                .Select(p => p.Housing);
            return await query.ToListAsync();
        }
    }
}