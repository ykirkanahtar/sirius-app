using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace Sirius.PaymentAccounts.Dto
{
    public class PagedAccountBookResultDto : PagedResultDto<AccountBookGetAllOutput>
    {
        public PagedAccountBookResultDto(int totalCount, IReadOnlyList<AccountBookGetAllOutput> items, decimal balance, DateTime? lastAccountBookDate = null)
            : base(totalCount, items)
        {
            Balance = balance;
            LastAccountBookDate = lastAccountBookDate;
        }
        public decimal Balance { get; }
        public DateTime? LastAccountBookDate { get; }
    }
}