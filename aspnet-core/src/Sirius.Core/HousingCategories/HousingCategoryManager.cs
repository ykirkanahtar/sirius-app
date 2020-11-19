using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sirius.Housings;

namespace Sirius.HousingCategories
{
    public class HousingCategoryManager : IHousingCategoryManager
    {
        private readonly IRepository<HousingCategory, Guid> _housingCategoryRepository;
        private readonly IRepository<Housing, Guid> _housingRepository;

        public HousingCategoryManager(IRepository<HousingCategory, Guid> housingCategoryRepository, IRepository<Housing, Guid> housingRepository)
        {
            _housingCategoryRepository = housingCategoryRepository;
            _housingRepository = housingRepository;
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
            var housings = await _housingRepository.GetAllListAsync(p => p.HousingCategoryId == housingCategory.Id);
            if (housings.Count > 0)
            {
                if (housings.Count == 1)
                {
                    var housing = await _housingRepository.GetAll().Include(p => p.Block).Where(p => p.Id == housings[0].Id).SingleAsync();
                    throw new UserFriendlyException($"Bu konut kategorisine ait {housings[0].GetName()} konutu tanımlıdır. Silmek için önce tanımı kaldırınız.");
                }
                else
                {
                    throw new UserFriendlyException("Bu konut kategorisi için birden fazla konut tanımlıdır. Silmek için önce tanımları kaldırınız.");
                }
            }

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
