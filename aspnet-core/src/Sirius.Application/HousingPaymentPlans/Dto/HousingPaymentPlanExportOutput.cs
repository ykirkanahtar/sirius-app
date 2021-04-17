using Abp.AutoMapper;

namespace Sirius.HousingPaymentPlans.Dto
{
    [AutoMapFrom(typeof(HousingPaymentPlan))]
    public class HousingPaymentPlanExportOutput
    {
        public string Date { get; set; }
        public string CreditOrDebt { get; set; }
        public string PaymentCategory { get; set; }
        public string HousingPaymentPlanType { get; set; }
        public decimal Amount { get; set; }
    }
}