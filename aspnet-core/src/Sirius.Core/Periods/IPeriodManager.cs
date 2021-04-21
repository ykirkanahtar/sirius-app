using System;
using System.Threading.Tasks;
using Abp.Domain.Services;

namespace Sirius.Periods
{
    public interface IPeriodManager : IDomainService
    {
        Task<Period> GetAsync(Guid id);
        Task CreateAsync(Period period);
        Task UpdateAsync(Period period);
        Task<Period> GetActivePeriod(bool nullCheck = true);
    }
}