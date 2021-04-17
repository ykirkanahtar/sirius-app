using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Sirius.Shared.Enums;

namespace Sirius.HousingPaymentPlans.Dto
{
    public interface IHousingPaymentPlanGetAllFilter : ISortedResultRequest
    {
        Guid HousingId { get; set; }
        DateTime? StartDateFilter { get; set; }
        DateTime? EndDateFilter { get; set; }
        List<Guid> PaymentCategoriesFilter { get; set; }
        List<CreditOrDebt> CreditOrDebtsFilter { get; set; }
        List<HousingPaymentPlanType> HousingPaymentPlanTypesFilter { get; set; }
    }
}