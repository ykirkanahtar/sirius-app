using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Microsoft.EntityFrameworkCore;
using Sirius.Housings.Dto;
using Sirius.Shared.Dtos;

namespace Sirius.Housings
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
            CheckCreatePermission();
            var housingCategory = HousingCategory.Create(SequentialGuidGenerator.Instance.Create(), AbpSession.GetTenantId(), input.HousingCategoryName);
            await _housingCategoryManager.CreateAsync(housingCategory);
            return ObjectMapper.Map<HousingCategoryDto>(housingCategory);
        }
        
        public override async Task<HousingCategoryDto> UpdateAsync(UpdateHousingCategoryDto input)
        {
            CheckUpdatePermission();
            var existingHousingCategory = await _housingCategoryManager.GetAsync(input.Id);
            var housingCategory = HousingCategory.Update(existingHousingCategory, input.HousingCategoryName);
            await _housingCategoryManager.UpdateAsync(housingCategory);
            return ObjectMapper.Map<HousingCategoryDto>(housingCategory);
        }

        public override async Task DeleteAsync(EntityDto<Guid> input)
        {
            CheckDeletePermission();

            var housingCategory = await _housingCategoryManager.GetAsync(input.Id);
            await _housingCategoryManager.DeleteAsync(housingCategory);
        }
        
        public override async Task<PagedResultDto<HousingCategoryDto>> GetAllAsync(
            PagedHousingCategoryResultRequestDto input)
        {
            CheckGetAllPermission();
            var query = _housingCategoryRepository
                .GetAll()
                .WhereIf(!string.IsNullOrWhiteSpace(input.HousingCategoryName),
                    p => p.HousingCategoryName.Contains(input.HousingCategoryName));
            
            var housingCategories = await query.OrderBy(input.Sorting ?? $"{nameof(HousingCategoryDto.HousingCategoryName)}")
                .PageBy(input)
                .ToListAsync();
            
            return new PagedResultDto<HousingCategoryDto>(query.Count(),
                ObjectMapper.Map<List<HousingCategoryDto>>(housingCategories));
        }

        public async Task<List<LookUpDto>> GetHousingCategoryLookUpAsync()
        {           
            CheckGetAllPermission();
            var housingCategories = await _housingCategoryRepository.GetAllListAsync();
            var selectList =  (from l in housingCategories
                select new LookUpDto(l.Id.ToString(), l.HousingCategoryName)).ToList();
            return selectList;
        }
        
        public async Task<List<string>> GetHousingCategoryFromAutoCompleteFilterAsync(string request)
        {
            CheckGetAllPermission();
            var query = from p in _housingCategoryRepository.GetAll()
                where p.HousingCategoryName.Contains(request)
                select p.HousingCategoryName;

            return await query.ToListAsync();
        }
    }
}
