using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Sirius.HousingCategories.Dto;
using Sirius.Shared.Dtos;

namespace Sirius.HousingCategories
{
    public class HousingCategoryAppService : AsyncCrudAppService<HousingCategory, HousingCategoryDto, Guid, PagedHousingCategoryResultRequestDto, CreateHousingCategoryDto, UpdateHousingCategoryDto>, IHousingCategoryAppService
    {
        private readonly IHousingCategoryManager _housingCategoryManager;
        private readonly IRepository<HousingCategory, Guid> _housingCategoryRepository;

        public HousingCategoryAppService(IHousingCategoryManager housingCategoryManager, IRepository<HousingCategory, Guid> housingCategoryRepository)
            : base(housingCategoryRepository)
        {
            _housingCategoryManager = housingCategoryManager;
            _housingCategoryRepository = housingCategoryRepository;
        }

        public override async Task<HousingCategoryDto> CreateAsync(CreateHousingCategoryDto input)
        {
            var housingCategory = HousingCategory.Create(SequentialGuidGenerator.Instance.Create(), AbpSession.GetTenantId(), input.HousingCategoryName);
            await _housingCategoryManager.CreateAsync(housingCategory);
            return ObjectMapper.Map<HousingCategoryDto>(housingCategory);
        }
        
        public override async Task<HousingCategoryDto> UpdateAsync(UpdateHousingCategoryDto input)
        {
            var existingHousingCategory = await _housingCategoryManager.GetAsync(input.Id);
            var housingCategory = HousingCategory.Update(existingHousingCategory, input.HousingCategoryName);
            await _housingCategoryManager.UpdateAsync(housingCategory);
            return ObjectMapper.Map<HousingCategoryDto>(housingCategory);
        }

        public override async Task DeleteAsync(EntityDto<Guid> input)
        {
            var housingCategory = await _housingCategoryManager.GetAsync(input.Id);
            await _housingCategoryManager.DeleteAsync(housingCategory);
        }

        public async Task<List<LookUpDto>> GetHousingCategoryLookUpAsync()
        {           
            var housingCategories = await _housingCategoryRepository.GetAllListAsync();
            var selectList =  (from l in housingCategories
                select new LookUpDto(l.Id.ToString(), l.HousingCategoryName)).ToList();
            return selectList;

        }
    }
}
