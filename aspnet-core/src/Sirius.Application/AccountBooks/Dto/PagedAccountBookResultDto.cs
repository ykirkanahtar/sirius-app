using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace Sirius.AccountBooks.Dto
{
    public class PagedAccountBookResultDto : PagedResultDto<AccountBookGetAllOutput>
    {
        public PagedAccountBookResultDto(int totalCount, IReadOnlyList<AccountBookGetAllOutput> items, DateTime? lastAccountBookDate = null)
            : base(totalCount, items)
        {
            LastAccountBookDate = lastAccountBookDate;
        }
        public DateTime? LastAccountBookDate { get; }
    }
}