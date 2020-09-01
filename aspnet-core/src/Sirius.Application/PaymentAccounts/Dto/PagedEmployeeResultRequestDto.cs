using Abp.Application.Services.Dto;

namespace Sirius.PaymentAccounts.Dto
{
    public class PagedPaymentAccountResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public bool? IsActive { get; set; }
    }
}