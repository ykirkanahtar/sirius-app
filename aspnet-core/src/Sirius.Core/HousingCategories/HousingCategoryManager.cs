using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.UI;

namespace Sirius.HousingCategories
{
    public class HousingCategoryManager : IHousingCategoryManager
    {
        private readonly IRepository<HousingCategory, Guid> _housingCategoryRepository;

        public HousingCategoryManager(IRepository<HousingCategory, Guid> housingCategoryRepository)
        {
            _housingCategoryRepository = housingCategoryRepository;
        }

        public async Task CreateAsync(HousingCategory housingCategory)
        {
            await _housingCategoryRepository.InsertAsync(housingCategory);
        }

        public async Task UpdateAsync(HousingCategory housingCategory)
        {
            await _housingCategoryRepository.UpdateAsync(housingCategory);
        }
        
        public async Task DeleteAsync(HousingCategory housingCategory)
        {
            await _housingCategoryRepository.DeleteAsync(housingCategory);
        }
        
        public async Task<HousingCategory> GetAsync(Guid id)
        {
            var housingCategory = await _housingCategoryRepository.GetAsync(id);
            if(housingCategory == null)
            {
                throw new UserFriendlyException("Konut kategorisi bulunamadı");
            }
            return housingCategory;
        }
    }
}
