using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace Sirius.HousingPaymentPlans.Dto
{
    public class PagedHousingPaymentPlanResultDto : PagedResultDto<HousingPaymentPlanDto>
    {
        public PagedHousingPaymentPlanResultDto(int totalCount,
            IReadOnlyList<HousingPaymentPlanDto> items, decimal balance, decimal creditBalance, decimal debtBalance)
            : base(totalCount, items)
        {
            Balance = balance;
            CreditBalance = creditBalance;
            DebtBalance = debtBalance;
        }

        public decimal Balance { get; }
        public decimal CreditBalance { get; }
        public decimal DebtBalance { get; }
    }
}