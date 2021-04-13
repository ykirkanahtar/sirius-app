using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using Sirius.Dashboard.Dto;
using Sirius.HousingPaymentPlans;
using Sirius.Housings;
using Sirius.PaymentAccounts;
using Sirius.PaymentCategories;
using Sirius.Periods;
using Sirius.Shared.Enums;

namespace Sirius.Dashboard
{
    public class DashboardAppService : IDashboardAppService
    {
        private readonly IRepository<PaymentAccount, Guid> _paymentAccountRepository;
        private readonly IRepository<Housing, Guid> _housingRepository;
        private readonly IRepository<PaymentCategory, Guid> _paymentCategoryRepository;
        private readonly IRepository<HousingPaymentPlanGroup, Guid> _housingPaymentPlanGroupRepository;
        private readonly IRepository<HousingPaymentPlan, Guid> _housingPaymentPlanRepository;
        private readonly IRepository<Block, Guid> _blockRepository;
        private readonly IRepository<AccountBook, Guid> _accountBookRepository;
        private readonly IRepository<Period, Guid> _periodRepository;

        public DashboardAppService(IRepository<PaymentAccount, Guid> paymentAccountRepository,
            IRepository<Housing, Guid> housingRepository, IRepository<PaymentCategory, Guid> paymentCategoryRepository,
            IRepository<HousingPaymentPlanGroup, Guid> housingPaymentPlanGroupRepository,
            IRepository<HousingPaymentPlan, Guid> housingPaymentPlanRepository,
            IRepository<Block, Guid> blockRepository, IRepository<AccountBook, Guid> accountBookRepository,
            IRepository<Period, Guid> periodRepository)
        {
            _paymentAccountRepository = paymentAccountRepository;
            _housingRepository = housingRepository;
            _paymentCategoryRepository = paymentCategoryRepository;
            _housingPaymentPlanGroupRepository = housingPaymentPlanGroupRepository;
            _housingPaymentPlanRepository = housingPaymentPlanRepository;
            _blockRepository = blockRepository;
            _accountBookRepository = accountBookRepository;
            _periodRepository = periodRepository;
        }

        public async Task<DashboardDto> GetDashboardData()
        {
            var dashboardDto = new DashboardDto();
            dashboardDto.PaymentAccounts = await _paymentAccountRepository.GetAll().Select(p =>
                new PaymentAccountDashboardDto
                {
                    PaymentAccountName = p.AccountName,
                    Balance = p.Balance
                }).OrderByDescending(p => p.Balance).ToListAsync();

            var currentPeriod =
                await _periodRepository.GetAll().OrderByDescending(p => p.StartDate).FirstOrDefaultAsync();

            if (currentPeriod != null)
            {
                var housingDueQuery = (from ab in _accountBookRepository.GetAll()
                        join pc in _paymentCategoryRepository.GetAll() on ab.PaymentCategoryId equals pc.Id
                        where pc.PaymentCategoryType == PaymentCategoryType.Income && pc.IsHousingDue &&
                              ab.ProcessDateTime >= currentPeriod.StartDate
                        select ab)
                    .WhereIf(currentPeriod.EndDate.HasValue, p => p.ProcessDateTime <= currentPeriod.EndDate.Value);

                var incomeQuery = (from ab in _accountBookRepository.GetAll()
                        join pc in _paymentCategoryRepository.GetAll() on ab.PaymentCategoryId equals pc.Id
                        where pc.PaymentCategoryType == PaymentCategoryType.Income && pc.IsHousingDue == false &&
                              ab.ProcessDateTime >= currentPeriod.StartDate
                        select ab)
                    .WhereIf(currentPeriod.EndDate.HasValue, p => p.ProcessDateTime <= currentPeriod.EndDate.Value);

                var expenseQuery = (from ab in _accountBookRepository.GetAll()
                        join pc in _paymentCategoryRepository.GetAll() on ab.PaymentCategoryId equals pc.Id
                        where pc.PaymentCategoryType == PaymentCategoryType.Expense &&
                              ab.ProcessDateTime >= currentPeriod.StartDate
                        select new {ab, pc})
                    .WhereIf(currentPeriod.EndDate.HasValue, p => p.ab.ProcessDateTime <= currentPeriod.EndDate.Value);

                dashboardDto.TotalHousingDueAmount = await housingDueQuery.SumAsync(p => p.Amount);
                dashboardDto.TotalIncomeAmount = await incomeQuery.SumAsync(p => p.Amount);
                dashboardDto.TotalExpenseAmount = await expenseQuery.SumAsync(p => p.ab.Amount);

                var housingsWithBlock = from h in _housingRepository.GetAll()
                    join b in _blockRepository.GetAll() on h.BlockId equals b.Id
                    select new
                    {
                        HousingId = h.Id,
                        HousingName = b.BlockName + " " + h.Apartment
                    };

                var housingDuePayersQuery = from h in housingsWithBlock
                    join a in housingDueQuery on h.HousingId equals a.HousingId into aNullable
                    from a in aNullable.DefaultIfEmpty()
                    select new
                    {
                        a.Amount,
                        h.HousingName
                    }
                    into grouped
                    group grouped by new {grouped.HousingName}
                    into housingNameGrouped
                    select new HousingDuePayersDashboardDto
                    {
                        TotalHousingDueAmount = housingNameGrouped.Sum(x => x.Amount),
                        HousingName = housingNameGrouped.Key.HousingName
                    };

                var expensesDataQuery = expenseQuery.GroupBy(x => new {x.pc.PaymentCategoryName})
                    .Select(p => new PaymentCategoryDashboardDto
                    {
                        TotalAmount = p.Sum(x => x.ab.Amount),
                        PaymentCategoryName = p.Key.PaymentCategoryName
                    });

                dashboardDto.MostHousingDuePayers =
                    await housingDuePayersQuery.OrderByDescending(p => p.TotalHousingDueAmount).Take(5)
                        .ToListAsync();

                dashboardDto.LessHousingDuePayers =
                    await housingDuePayersQuery.OrderBy(p => p.TotalHousingDueAmount).Take(5)
                        .ToListAsync();

                dashboardDto.ExpensesData =
                    await expensesDataQuery.OrderByDescending(p => p.TotalAmount)
                        .ToListAsync();
            }

            return dashboardDto;
        }
    }
}