using System;
using Abp.Application.Services.Dto;

namespace Sirius.HousingPaymentPlans.Dto
{
    public class PagedHousingPaymentPlanResultRequestDto : PagedAndSortedResultRequestDto
    {
        public Guid HousingId { get; set; }
    }
}