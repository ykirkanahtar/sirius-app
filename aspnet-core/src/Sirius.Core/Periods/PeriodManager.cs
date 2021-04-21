using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;

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
            await _periodRepository.InsertAsync(period);
        }

        public async Task UpdateAsync(Period period)
        {
            await _periodRepository.UpdateAsync(period);
        }

        public async Task<Period> GetActivePeriod(bool nullCheck = true)
        {
            //Active olan dönem bulunuyor ve kayıtlarda varsa kapatılıyor
            var period = await _periodRepository.GetAll()
                .Where(p => p.IsActive)
                // .WhereIf(siteOrBlock == SiteOrBlock.Block,
                //     p => p.SiteOrBlock == SiteOrBlock.Block && p.BlockId == period.BlockId)
                // .WhereIf(siteOrBlock == SiteOrBlock.Site, p => p.SiteOrBlock == SiteOrBlock.Site)
                .SingleOrDefaultAsync();

            if (period == null && nullCheck)
            {
                throw new UserFriendlyException("Sistemde aktif bir dönem bulunamadı, lütfen dönem tanımlayın.");
            }

            return period;
        }
    }
}