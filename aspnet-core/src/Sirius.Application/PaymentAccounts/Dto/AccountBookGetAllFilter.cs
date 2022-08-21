using System;
using System.Collections.Generic;

namespace Sirius.AccountBooks.Dto
{
    public class AccountBookGetAllFilter : IAccountBookGetAllFilter
    {
        public AccountBookGetAllFilter()
        {
            PaymentCategoryIds = new List<Guid>();
            HousingIds = new List<Guid>();
            PersonIds = new List<Guid>();
            FromPaymentAccountIds = new List<Guid>();
            ToPaymentAccountIds = new List<Guid>();
        }

        public Guid? PeriodId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<Guid> PaymentCategoryIds { get; set; }
        public List<Guid> HousingIds { get; set; }
        public List<Guid> PersonIds { get; set; }
        public List<Guid> FromPaymentAccountIds { get; set; }
        public List<Guid> ToPaymentAccountIds { get; set; }
        public string Sorting { get; set; }
    }
}