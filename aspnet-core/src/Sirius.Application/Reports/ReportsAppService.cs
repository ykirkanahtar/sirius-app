using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Sirius.HousingPaymentPlans;
using Sirius.Housings;
using Sirius.Periods;
using Sirius.Reports.Dto;

namespace Sirius.Reports
{
    public class ReportsAppService : ApplicationService, IReportsAppService
    {
        private readonly IRepository<Housing, Guid> _housingRepository;
        private readonly IRepository<HousingPaymentPlan, Guid> _housingPaymentPlanRepository;
        private readonly IRepository<HousingPaymentPlanGroup, Guid> _housingPaymentPlanGroupRepository;
        private readonly IRepository<Block, Guid> _blockRepository;
        private readonly IRepository<Period, Guid> _periodRepository;

        public ReportsAppService(IRepository<Housing, Guid> housingRepository,
            IRepository<HousingPaymentPlan, Guid> housingPaymentPlanRepository,
            IRepository<Block, Guid> blockRepository, IRepository<Period, Guid> periodRepository,
            IRepository<HousingPaymentPlanGroup, Guid> housingPaymentPlanGroupRepository)
        {
            _housingRepository = housingRepository;
            _housingPaymentPlanRepository = housingPaymentPlanRepository;
            _blockRepository = blockRepository;
            _periodRepository = periodRepository;
            _housingPaymentPlanGroupRepository = housingPaymentPlanGroupRepository;
        }

        public async Task<List<HousingDueReportDto>> GetHousingDueReport(HousingDueReportFilter filter)
        {
            filter.PeriodId = await _periodRepository.GetAll().OrderByDescending(p => p.Id).Take(1).Select(p => p.Id)
                .SingleAsync();

            var baseQuery =
                from housing in _housingRepository.GetAll()
                join block in _blockRepository.GetAll() on housing.BlockId equals block.Id
                join housingPaymentPlan in _housingPaymentPlanRepository.GetAll() on housing.Id equals
                    housingPaymentPlan.HousingId
                join housingPaymentPlanGroup in _housingPaymentPlanGroupRepository.GetAll() on housingPaymentPlan
                    .HousingPaymentPlanGroupId equals housingPaymentPlanGroup.Id
                from period in _periodRepository.GetAll().OrderByDescending(p => p.Id).Take(1)
                where period.StartDate < housingPaymentPlan.Date &&
                      (period.EndDate > housingPaymentPlan.Date || period.EndDate == null)
                select new
                {
                    HousingId = housing.Id,
                    block.BlockName,
                    housing.Apartment,
                    housing.Balance,
                    housingPaymentPlan.Date.Month,
                    housingPaymentPlan.Date.Year,
                    housingPaymentPlan.Amount
                };

            var balanceQuery = from q in baseQuery
                group q by new
                {
                    q.HousingId,
                    q.BlockName,
                    q.Apartment,
                    q.Balance
                }
                into grouping
                select new HousingDueReportDto
                {
                    PeriodName = string.Empty,
                    HousingId = grouping.Key.HousingId,
                    BlockName = grouping.Key.BlockName,
                    Apartment = grouping.Key.Apartment,
                    TotalBalance = 0,
                    CurrentBalance = grouping.Key.Balance,
                    BalanceOfTheBeginningOfThePeriod = 0,
                };

            var balanceDetailQuery = from q in baseQuery
                select new HousingDueReportDetailDto
                {
                    HousingId = q.HousingId,
                    Month = q.Month,
                    Year = q.Year,
                    Amount = q.Amount
                };

            var balances = await balanceQuery.ToListAsync();
            var balanceDetails = await balanceDetailQuery.ToListAsync();

            foreach (var balance in balances)
            {
                balance.HousingDueReportDetails = balanceDetails
                    .Where(p => p.HousingId == balance.HousingId)
                    .Select(p => new HousingDueReportDetailDto
                    {
                        HousingId = p.HousingId,
                        Month = p.Month,
                        Year = p.Year,
                        Amount = p.Amount
                    }).ToList();
            }

            return balances;
        }
    }
}