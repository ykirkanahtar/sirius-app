using System;
using System.Collections.Generic;

namespace Sirius.AccountBooks.Dto
{
    public interface IAccountBookGetAllFilter
    {
        DateTime? StartDate { get; set; }
        DateTime? EndDate { get; set; }
        List<Guid> PaymentCategoryIds { get; set; }
        List<Guid> HousingIds { get; set; }
        List<Guid> PersonIds { get; set; }
        List<Guid> FromPaymentAccountIds { get; set; }
        List<Guid> ToPaymentAccountIds { get; set; }
        string Sorting { get; set; }
    }
}