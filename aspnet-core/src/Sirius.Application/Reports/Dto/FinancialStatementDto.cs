using System.Collections.Generic;
using Abp.Runtime.Validation;

namespace Sirius.Dashboard.Dto
{
    public class FinancialStatementDto
    {
        public FinancialStatementDto()
        {
            Title = new List<string>();
            Incomes = new List<ReportLine>();
            InitialAmounts = new List<ReportLine>();
            Expenses = new List<ReportLine>();
            FinallyAmounts = new List<ReportLine>();
        }

        public List<string> Title { get; set; }
        public string IncomesTitle { get; set; }
        public List<ReportLine> Incomes { get; set; }
        public List<ReportLine> InitialAmounts { get; set; }
        public ReportLine IncomeTotal { get; set; }
        public ReportLine IncomeTotalWithInitialAmounts { get; set; }

        public string ExpensesTitle { get; set; }
        public List<ReportLine> Expenses { get; set; }
        public List<ReportLine> FinallyAmounts { get; set; }
        public ReportLine ExpenseTotal { get; set; }
        public ReportLine ExpenseTotalWithFinallyAmounts { get; set; }
    }
}