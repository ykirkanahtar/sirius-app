using System;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Sirius.Housings.Dto;

namespace Sirius.Housings
{
    public class HousingAppService : AsyncCrudAppService<Housing, HousingDto, Guid, PagedHousingResultRequestDto, CreateHousingDto, UpdateHousingDto>, IHousingAppService
    {
        private readonly IHousingManager _housingManager;

        public HousingAppService(IHousingManager housingManager, IRepository<Housing, Guid> housingRepository)
            : base(housingRepository)
        {
            _housingManager = housingManager;
        }

        public override async Task<HousingDto> CreateAsync(CreateHousingDto input)
        {
            var housing = Housing.Create(SequentialGuidGenerator.Instance.Create(), AbpSession.GetTenantId(), input.Block, input.Apartment);
            await _housingManager.CreateAsync(housing);
            return ObjectMapper.Map<HousingDto>(housing);
        }
        
        public override async Task<HousingDto> UpdateAsync(UpdateHousingDto input)
        {
            var existingHousing = await _housingManager.GetAsync(input.Id);
            var housing = Housing.Update(existingHousing, input.Block, input.Apartment);
            await _housingManager.UpdateAsync(housing);
            return ObjectMapper.Map<HousingDto>(housing);
        }

        public override async Task DeleteAsync(EntityDto<Guid> input)
        {
            var housing = await _housingManager.GetAsync(input.Id);
            await _housingManager.DeleteAsync(housing);
        }
    }
}
