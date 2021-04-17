using System;
using System.Collections.Generic;
using Sirius.Shared.Enums;

namespace Sirius.HousingPaymentPlans.Dto
{
    public class HousingPaymentPlanGetAllFilter : IHousingPaymentPlanGetAllFilter
    {
        public HousingPaymentPlanGetAllFilter()
        {
            PaymentCategoriesFilter = new List<Guid>();
            CreditOrDebtsFilter = new List<CreditOrDebt>();
            HousingPaymentPlanTypesFilter = new List<HousingPaymentPlanType>();
        }
        public Guid HousingId { get; set; }
        public DateTime? StartDateFilter { get; set; }
        public DateTime? EndDateFilter { get; set; }
        public List<Guid> PaymentCategoriesFilter { get; set; }
        public List<CreditOrDebt> CreditOrDebtsFilter { get; set; }
        public List<HousingPaymentPlanType> HousingPaymentPlanTypesFilter { get; set; }
        public string Sorting { get; set; }
    }
}