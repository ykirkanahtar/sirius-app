using Abp.Application.Services.Dto;

namespace Sirius.HousingPaymentPlans.Dto
{
    public class PagedHousingPaymentPlanResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public bool? IsActive { get; set; }
    }
}