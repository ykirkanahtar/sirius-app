using System;
using Abp.Application.Services.Dto;

namespace Sirius.HousingPaymentPlans.Dto
{
    public class PagedHousingPaymentPlanResultRequestDto : PagedResultRequestDto
    {
        public Guid HousingId { get; set; }
        public string Keyword { get; set; }
        public bool? IsActive { get; set; }
    }
}