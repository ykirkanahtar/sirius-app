using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Sirius.HousingCategories;
using Sirius.Housings.Dto;
using Sirius.Shared.Dtos;

namespace Sirius.Housings
{
    public class HousingAppService : AsyncCrudAppService<Housing, HousingDto, Guid, PagedHousingResultRequestDto, CreateHousingDto, UpdateHousingDto>, IHousingAppService
    {
        private readonly IHousingManager _housingManager;
        private readonly IRepository<Housing, Guid> _housingRepository;
        private readonly IRepository<HousingCategory, Guid> _housingCategoryRepository;
        public HousingAppService(IHousingManager housingManager, IRepository<Housing, Guid> housingRepository, IRepository<HousingCategory, Guid> housingCategoryRepository)
            : base(housingRepository)
        {
            _housingManager = housingManager;
            _housingRepository = housingRepository;
            _housingCategoryRepository = housingCategoryRepository;
        }

        public override async Task<HousingDto> CreateAsync(CreateHousingDto input)
        {
            var housingCategory = await _housingCategoryRepository.GetAsync(input.HousingCategoryId);
            
            var housing = Housing.Create(SequentialGuidGenerator.Instance.Create(), AbpSession.GetTenantId(), input.Block, input.Apartment, housingCategory);
            await _housingManager.CreateAsync(housing);
            return ObjectMapper.Map<HousingDto>(housing);
        }
        
        public override async Task<HousingDto> UpdateAsync(UpdateHousingDto input)
        {
            var existingHousing = await _housingManager.GetAsync(input.Id);
            var housingCategory = await _housingCategoryRepository.GetAsync(input.HousingCategoryId);

            var housing = Housing.Update(existingHousing, input.Block, input.Apartment, housingCategory);
            await _housingManager.UpdateAsync(housing);
            return ObjectMapper.Map<HousingDto>(housing);
        }

        public override async Task DeleteAsync(EntityDto<Guid> input)
        {
            var housing = await _housingManager.GetAsync(input.Id);
            await _housingManager.DeleteAsync(housing);
        }

        public async Task<List<LookUpDto>> GetHousingLookUpAsync()
        {           
            var housings = await _housingRepository.GetAllListAsync();
            
            return
                (from l in housings
                select new LookUpDto(l.Id.ToString(), l.GetName())).ToList();                                                     
        }
    }
}
