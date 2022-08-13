using System;

namespace Sirius.Dashboard.Dto
{
    public class HousingDueBalancesDashboardDto
    {
        public Guid HousingId { get; set; }
        public string HousingName { get; set; }
        public decimal Balance { get; set; }
    }
}