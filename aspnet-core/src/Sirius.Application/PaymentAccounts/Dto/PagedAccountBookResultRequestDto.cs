using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace Sirius.AccountBooks.Dto
{
    public class PagedAccountBookResultRequestDto : PagedAndSortedResultRequestDto
    {
        public PagedAccountBookResultRequestDto()
        {
            PaymentCategoryIds = new List<Guid>();
            HousingIds = new List<Guid>();
            PersonIds = new List<Guid>();
            FromPaymentAccountIds = new List<Guid>();
            ToPaymentAccountIds = new List<Guid>();
        }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<Guid> PaymentCategoryIds { get; set; }
        public List<Guid> HousingIds { get; set; }
        public List<Guid> PersonIds { get; set; }
        public List<Guid> FromPaymentAccountIds { get; set; }
        public List<Guid> ToPaymentAccountIds { get; set; }
    }
}