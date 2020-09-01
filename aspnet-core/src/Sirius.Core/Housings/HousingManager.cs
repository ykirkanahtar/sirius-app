using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.UI;

namespace Sirius.Housings
{
    public class HousingManager : IHousingManager
    {
        private readonly IRepository<Housing, Guid> _housingRepository;

        public HousingManager(IRepository<Housing, Guid> housingRepository)
        {
            _housingRepository = housingRepository;
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
        
        public async Task<Housing> GetAsync(Guid id)
        {
            var housing = await _housingRepository.GetAsync(id);
            if(housing == null)
            {
                throw new UserFriendlyException("Konut bulunamadı");
            }
            return housing;
        }
    }
}
