using Abp.Application.Services.Dto;

namespace Sirius.PaymentCategories.Dto
{
    public class PagedPaymentCategoryResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public bool? IsActive { get; set; }
    }
}