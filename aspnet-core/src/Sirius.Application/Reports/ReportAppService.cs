using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sirius.Dashboard.Dto;
using Sirius.HousingPaymentPlans;
using Sirius.Housings;
using Sirius.MultiTenancy;
using Sirius.PaymentAccounts;
using Sirius.PaymentCategories;
using Sirius.Periods;
using Sirius.Shared.Enums;

namespace Sirius.Reports
{
    public class ReportAppService : SiriusAppServiceBase, IReportAppService
    {
        private readonly IRepository<PaymentAccount, Guid> _paymentAccountRepository;
        private readonly IRepository<Housing, Guid> _housingRepository;
        private readonly IRepository<PaymentCategory, Guid> _paymentCategoryRepository;
        private readonly IRepository<HousingPaymentPlan, Guid> _housingPaymentPlanRepository;
        private readonly IRepository<Block, Guid> _blockRepository;
        private readonly IRepository<AccountBook, Guid> _accountBookRepository;
        private readonly IRepository<Period, Guid> _periodRepository;
        private readonly IRepository<Tenant> _tenantRepository;

        public ReportAppService(IRepository<PaymentAccount, Guid> paymentAccountRepository,
            IRepository<Housing, Guid> housingRepository, IRepository<PaymentCategory, Guid> paymentCategoryRepository,
            IRepository<HousingPaymentPlan, Guid> housingPaymentPlanRepository,
            IRepository<Block, Guid> blockRepository, IRepository<AccountBook, Guid> accountBookRepository,
            IRepository<Period, Guid> periodRepository, IRepository<Tenant> tenantRepository)
        {
            _paymentAccountRepository = paymentAccountRepository;
            _housingRepository = housingRepository;
            _paymentCategoryRepository = paymentCategoryRepository;
            _housingPaymentPlanRepository = housingPaymentPlanRepository;
            _blockRepository = blockRepository;
            _accountBookRepository = accountBookRepository;
            _periodRepository = periodRepository;
            _tenantRepository = tenantRepository;
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
                var housingDueDefinitionSum = await
                    _housingPaymentPlanRepository.GetAll().Where(p =>
                            p.Date >= currentPeriod.StartDate && p.HousingPaymentPlanType ==
                            HousingPaymentPlanType.HousingDueDefinition)
                        .WhereIf(currentPeriod.EndDate.HasValue, p => p.Date <= currentPeriod.EndDate.Value)
                        .SumAsync(p => p.Amount);

                var housingDuePaymentSum = await
                    _housingPaymentPlanRepository.GetAll().Where(p =>
                            p.Date >= currentPeriod.StartDate && p.HousingPaymentPlanType ==
                            HousingPaymentPlanType.HousingDuePayment)
                        .WhereIf(currentPeriod.EndDate.HasValue, p => p.Date <= currentPeriod.EndDate.Value)
                        .SumAsync(p => p.Amount);

                dashboardDto.TotalHousingDueStatsDto = new TotalHousingDueStatsDto
                {
                    TotalHousingDueDefinition = housingDueDefinitionSum,
                    TotalHousingDuePayment = housingDuePaymentSum
                };

                var housingDueQuery = from ab in _accountBookRepository.GetAll()
                    join p in _periodRepository.GetAll() on ab.PeriodId equals p.Id
                    where p.IsActive &&
                          (ab.AccountBookType == AccountBookType.HousingDue || ab.AccountBookType ==
                              AccountBookType.OtherPaymentWithNettingForHousingDue)
                    select ab;

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
                        select new { ab, pc })
                    .WhereIf(currentPeriod.EndDate.HasValue, p => p.ab.ProcessDateTime <= currentPeriod.EndDate.Value);

                dashboardDto.TotalHousingDueAmount = await housingDueQuery.SumAsync(p => p.Amount);
                dashboardDto.TotalIncomeAmount = await incomeQuery.SumAsync(p => p.Amount);
                dashboardDto.TotalExpenseAmount = await expenseQuery.SumAsync(p => p.ab.Amount);

                var housingsWithBlock = from h in _housingRepository.GetAll()
                    join b in _blockRepository.GetAll() on h.BlockId equals b.Id
                    select new HousingDueBalancesDashboardDto
                    {
                        HousingId = h.Id,
                        HousingName = b.BlockName + " " + h.Apartment,
                        Balance = h.Balance
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
                    group grouped by new { grouped.HousingName }
                    into housingNameGrouped
                    select new HousingDuePayersDashboardDto
                    {
                        TotalHousingDueAmount = housingNameGrouped.Sum(x => x.Amount),
                        HousingName = housingNameGrouped.Key.HousingName
                    };

                var expensesDataQuery = expenseQuery.GroupBy(x => new { x.pc.PaymentCategoryName })
                    .Select(p => new PaymentCategoryDashboardDto
                    {
                        TotalAmount = p.Sum(x => x.ab.Amount),
                        PaymentCategoryName = p.Key.PaymentCategoryName
                    });

                dashboardDto.MostHousingDueBalances =
                    await housingsWithBlock.OrderByDescending(p => p.Balance).Take(5)
                        .ToListAsync();

                dashboardDto.LessHousingDueBalances =
                    await housingsWithBlock.OrderBy(p => p.Balance).Take(5)
                        .ToListAsync();

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

        public async Task<FinancialStatementDto> GetFinancialStatement()
        {
            var activePeriod = await _periodRepository.GetAll()
                .Where(p => p.IsActive)
                .SingleAsync();

            var accountBooksQuery = from ab in _accountBookRepository.GetAll()
                join p in _periodRepository.GetAll() on ab.PeriodId equals p.Id
                join fpa in _paymentAccountRepository.GetAll() on ab.FromPaymentAccountId equals fpa.Id into nullableFpa
                from fpa in nullableFpa.DefaultIfEmpty()
                join tpa in _paymentAccountRepository.GetAll() on ab.ToPaymentAccountId equals tpa.Id into nullableTpa
                from tpa in nullableTpa.DefaultIfEmpty()
                join pc in _paymentCategoryRepository.GetAll() on ab.PaymentCategoryId equals pc.Id into nullablePc
                from pc in nullablePc.DefaultIfEmpty()
                where p.IsActive && ab.IsDeleted == false
                select new
                {
                    AccountBook = ab,
                    FromPaymentAccount = fpa,
                    ToPaymentAccount = tpa,
                    PaymentCategory = pc
                };

            var housingDues = await accountBooksQuery
                .Where(p => p.AccountBook.AccountBookType == AccountBookType.HousingDue ||
                            p.AccountBook.AccountBookType == AccountBookType.OtherPaymentWithNettingForHousingDue)
                .SumAsync(p => p.AccountBook.Amount);

            var transfersFromPrevPeriod = await (accountBooksQuery
                    .Where(p => p.AccountBook.AccountBookType ==
                                AccountBookType.TransferForPaymentAccountFromPreviousPeriod)
                    .Select(p => new
                    {
                        AccountBookName = p.ToPaymentAccount.AccountName,
                        Balance = p.AccountBook.Amount
                    }))
                .ToListAsync();

            var sumOfTransfersFromPrevPeriod = transfersFromPrevPeriod
                .Sum(p => p.Balance);

            var totalIncomes = housingDues + sumOfTransfersFromPrevPeriod;

            var expenses = await (from ab in accountBooksQuery
                    where
                        ab.PaymentCategory.PaymentCategoryType == PaymentCategoryType.Expense &&
                        (ab.AccountBook.AccountBookType == AccountBookType.Other ||
                         ab.AccountBook.AccountBookType == AccountBookType.OtherPaymentWithNettingForHousingDue)
                    group ab by new
                    {
                        PaymentCategoryId = ab.PaymentCategory.Id,
                        ab.PaymentCategory.PaymentCategoryName
                    }
                    into grp
                    select new
                    {
                        grp.Key.PaymentCategoryName,
                        Amount = grp.Sum(p => p.AccountBook.Amount)
                    })
                .ToListAsync();

            var sumOfExpenses = expenses.Sum(p => p.Amount);

            var paymentAccounts = await _paymentAccountRepository.GetAll().ToListAsync();
            var paymentAccountBalances = new Dictionary<string, decimal>();

            foreach (var paymentAccount in paymentAccounts)
            {
                var lastAmountQueryForFromPaymentAccount = _accountBookRepository.GetAll()
                    .Where(p => p.PeriodId == activePeriod.Id && p.FromPaymentAccountId == paymentAccount.Id)
                    .Select(p => new
                        { p.ProcessDateTime, p.SameDayIndex, Balance = p.FromPaymentAccountCurrentBalance ?? 0 })
                    .OrderByDescending(p => p.ProcessDateTime)
                    .ThenByDescending(p => p.SameDayIndex)
                    .Take(1);

                var lastAmountQueryForToPaymentAccount = _accountBookRepository.GetAll()
                    .Where(p => p.PeriodId == activePeriod.Id && p.ToPaymentAccountId == paymentAccount.Id)
                    .Select(p => new
                        { p.ProcessDateTime, p.SameDayIndex, Balance = p.ToPaymentAccountCurrentBalance ?? 0 })
                    .OrderByDescending(p => p.ProcessDateTime)
                    .ThenByDescending(p => p.SameDayIndex)
                    .Take(1);

                var lastAmountForPaymentAccount = await lastAmountQueryForFromPaymentAccount
                    .Union(lastAmountQueryForToPaymentAccount)
                    .Select(p => new { p.ProcessDateTime, p.SameDayIndex, paymentAccount.AccountName, p.Balance })
                    .OrderByDescending(p => p.ProcessDateTime)
                    .ThenByDescending(p => p.SameDayIndex)
                    .Take(1)
                    .SingleOrDefaultAsync();

                paymentAccountBalances.Add(lastAmountForPaymentAccount.AccountName,
                    lastAmountForPaymentAccount.Balance);
            }

            var expenseTotalWithFinallyAmount = sumOfExpenses + paymentAccountBalances.Sum(p => p.Value);

            if (activePeriod.EndDate == null)
            {
                throw new UserFriendlyException("Lütfen geçerli dönem için bitiş tarihi giriniz.");
            }

            var startDateText = activePeriod.StartDate.ToString("dd.MMMM.yyyy");
            var endDateText = activePeriod.EndDate.Value.ToString("dd.MMMM.yyyy");

            var tenant = await _tenantRepository.GetAsync(AbpSession.GetTenantId());
            var housingDueText =
                $"Bankaya ödenen {activePeriod.StartDate.Year}-{activePeriod.EndDate.Value.Year} aidatlar";

            var financialStatement = new FinancialStatementDto();
            financialStatement.Title.Add(tenant.Name.ToUpperInvariant());
            financialStatement.Title.Add(L("FinancialStatementsTitle").ToUpperInvariant());

            financialStatement.Title.Add(
                $"{startDateText.ToUpperInvariant()} - {endDateText.ToUpperInvariant()}");

            financialStatement.IncomesTitle = L("Incomes").ToUpperInvariant();

            financialStatement.Incomes.Add(new ReportLine(housingDueText, SetAmount(housingDues)));

            financialStatement.IncomeTotal = new ReportLine(L("IncomesTotal"), SetAmount(housingDues));

            transfersFromPrevPeriod.ForEach(item =>
            {
                financialStatement.InitialAmounts.Add(new ReportLine(
                    L("FinancialStatementsInitialBalanceText", item.AccountBookName, startDateText),
                    SetAmount(item.Balance)));
            });

            financialStatement.IncomeTotalWithInitialAmounts =
                new ReportLine(L("Total"), SetAmount(totalIncomes));

            financialStatement.ExpensesTitle = L("Expenses").ToUpperInvariant();

            expenses.ForEach(item =>
            {
                financialStatement.Expenses.Add(new ReportLine(item.PaymentCategoryName,
                    SetAmount(item.Amount)));
            });

            financialStatement.ExpenseTotal = new ReportLine(L("ExpensesTotal"), SetAmount(sumOfExpenses));

            foreach (var paymentAccountBalance in paymentAccountBalances)
            {
                financialStatement.FinallyAmounts.Add(new ReportLine(
                    L("FinancialStatementsFinallyBalanceText", paymentAccountBalance.Key, endDateText),
                    SetAmount(paymentAccountBalance.Value)));
            }

            financialStatement.ExpenseTotalWithFinallyAmounts =
                new ReportLine(L("Total"), SetAmount(expenseTotalWithFinallyAmount));

            return financialStatement;
        }

        private static string SetAmount(decimal amount)
        {
            return $"{amount:#,##0.##}_TL";
        }
    }
}