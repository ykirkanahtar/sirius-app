using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using Sirius.Periods.Dto;
using Sirius.Shared.Dtos;

namespace Sirius.Periods
{
    public interface IPeriodAppService : IAsyncCrudAppService<PeriodDto, Guid, PagedPeriodResultRequestDto,
        CreatePeriodDto, UpdatePeriodDto>
    {
        Task<List<LookUpDto>> GetPeriodLookUpAsync();
    }

    public class PeriodAppService :
        AsyncCrudAppService<Period, PeriodDto, Guid, PagedPeriodResultRequestDto, CreatePeriodDto, UpdatePeriodDto>,
        IPeriodAppService
    {
        private readonly IRepository<Period, Guid> _periodRepository;
        private readonly IPeriodManager _periodManager;

        public PeriodAppService(IRepository<Period, Guid> periodRepository, IPeriodManager periodManager) : base(
            periodRepository)
        {
            _periodRepository = periodRepository;
            _periodManager = periodManager;
        }

        public override async Task<PeriodDto> CreateAsync(CreatePeriodDto input)
        {
            CheckCreatePermission();
            var period = Period.Create(
                SequentialGuidGenerator.Instance.Create()
                , AbpSession.GetTenantId()
                , input.Name
                , input.StartDate
            );
            await _periodManager.CreateAsync(period);
            return ObjectMapper.Map<PeriodDto>(period);
        }

        public override async Task<PeriodDto> UpdateAsync(UpdatePeriodDto input)
        {
            CheckUpdatePermission();
            var existingPeriod = await _periodManager.GetAsync(input.Id);
            var period = Period.Update(existingPeriod, input.Name);
            await _periodManager.UpdateAsync(period);
            return ObjectMapper.Map<PeriodDto>(period);
        }

        public override Task DeleteAsync(EntityDto<Guid> input)
        {
            CheckDeletePermission();
            throw new NotSupportedException();
        }

        public override async Task<PagedResultDto<PeriodDto>> GetAllAsync(PagedPeriodResultRequestDto input)
        {
            CheckGetAllPermission();
            var query = _periodRepository.GetAll()
                .WhereIf(!string.IsNullOrWhiteSpace(input.Name),
                    p => (p.Name == input.Name));

            var periods = await query
                .OrderBy(input.Sorting ?? $"{nameof(PeriodDto.Name)}")
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<PeriodDto>(query.Count(),
                ObjectMapper.Map<List<PeriodDto>>(periods));
        }

        public async Task<List<LookUpDto>> GetPeriodLookUpAsync()
        {
            CheckGetAllPermission();

            var period = await _periodRepository.GetAllListAsync();

            return
                (from l in period
                    select new LookUpDto(l.Id.ToString(), l.Name)).ToList();
        }
    }
}