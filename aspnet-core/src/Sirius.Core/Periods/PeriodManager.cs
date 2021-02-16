using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sirius.Shared.Enums;

namespace Sirius.Periods
{
    public class PeriodManager : IPeriodManager
    {
        private readonly IRepository<Period, Guid> _periodRepository;

        public PeriodManager(IRepository<Period, Guid> periodRepository)
        {
            _periodRepository = periodRepository;
        }

        public async Task<Period> GetAsync(Guid id)
        {
            var period = await _periodRepository.GetAsync(id);
            if (period == null)
            {
                throw new UserFriendlyException("Dönem bulunamadı");
            }

            return period;
        }

        public async Task CreateAsync(Period period)
        {
            //Active olan dönem bulunuyor ve kayıtlarda varsa kapatılıyor
            var activePeriod = await _periodRepository.GetAll().Where(p => p.IsActive)
                .WhereIf(period.PeriodFor == PeriodFor.Block,
                    p => p.PeriodFor == PeriodFor.Block && p.BlockId == period.BlockId)
                .WhereIf(period.PeriodFor == PeriodFor.Site, p => p.PeriodFor == PeriodFor.Site)
                .SingleOrDefaultAsync();

            activePeriod?.ClosePeriod(activePeriod, period.StartDate);

            await _periodRepository.InsertAsync(period);
        }

        public async Task UpdateAsync(Period period)
        {
            await _periodRepository.UpdateAsync(period);
        }
    }
}