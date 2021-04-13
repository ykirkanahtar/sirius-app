using System.Collections.Generic;

namespace Sirius.Dashboard.Dto
{
    public class DashboardDto
    {
        public DashboardDto()
        {
            PaymentAccounts = new List<PaymentAccountDashboardDto>();
            MostHousingDuePayers = new List<HousingDuePayersDashboardDto>();
            LessHousingDuePayers = new List<HousingDuePayersDashboardDto>();
            ExpensesData = new List<PaymentCategoryDashboardDto>();
            TotalHousingDueStatsDto = new TotalHousingDueStatsDto();
        }
        
        public List<PaymentAccountDashboardDto> PaymentAccounts { get; set; }
        public decimal TotalHousingDueAmount { get; set; }
        public decimal TotalIncomeAmount { get; set; }
        public decimal TotalExpenseAmount { get; set; }
        public List<HousingDuePayersDashboardDto> MostHousingDuePayers { get; set; }
        public List<HousingDuePayersDashboardDto> LessHousingDuePayers { get; set; }
        public List<PaymentCategoryDashboardDto> ExpensesData { get; set; }
        public TotalHousingDueStatsDto TotalHousingDueStatsDto { get; set; }
    }
}