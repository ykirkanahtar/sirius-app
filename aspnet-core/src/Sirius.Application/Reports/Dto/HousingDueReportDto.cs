using System;
using System.Collections.Generic;

namespace Sirius.Reports.Dto
{
    public class HousingDueReportFilter
    {
        public Guid PeriodId { get; set; }
    }

    public class HousingDueReportDto
    {
        public HousingDueReportDto()
        {
            HousingDueReportDetails = new List<HousingDueReportDetailDto>();
        }

        public string PeriodName { get; set; }
        public Guid HousingId { get; set; }
        public string BlockName { get; set; }
        public string Apartment { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal BalanceOfTheBeginningOfThePeriod { get; set; }
        public List<HousingDueReportDetailDto> HousingDueReportDetails { get; set; }
    }

    public class HousingDueReportDetailDto
    {
        public Guid HousingId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal? Amount { get; set; }
    }
}