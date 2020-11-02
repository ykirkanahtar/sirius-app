using Abp.Application.Services.Dto;

namespace Sirius.PaymentCategories.Dto
{
    public class PagedPaymentCategoryResultRequestDto : PagedAndSortedResultRequestDto
    {
        public string PaymentCategoryName { get; set; }
    }
}