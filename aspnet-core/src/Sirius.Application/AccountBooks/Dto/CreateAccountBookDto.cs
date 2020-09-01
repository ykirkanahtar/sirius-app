﻿using System;
using Abp.AutoMapper;

namespace Sirius.AccountBooks.Dto
{
    [AutoMapTo(typeof(AccountBook))]
    public class CreateAccountBookDto
    {
        public DateTime ProcessDateTime { get; set; }
        public Guid? HousingId { get; set; }
        public Guid? FromPaymentAccountId { get; set; }
        public Guid? ToPaymentAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime? DocumentDateTime { get; set; }
        public string DocumentNumber { get; set; }
    }

    // [AutoMapTo(typeof(AccountBook))]
    // public class CreateAccountBookDto
    // {
    //     public DateTime ProcessDateTime { get; set; }
    //     public Guid FromPaymentAccountId { get; set; }
    //     public Guid ToPaymentAccountId { get; set; }
    //     public decimal Amount { get; set; }
    //     public string Description { get; set; }
    // }
}