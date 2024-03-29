﻿using System;
using System.Collections.Generic;
using Abp.AutoMapper;
using Abp.Runtime.Validation;
using Sirius.Inventories.Dto;
using Sirius.Shared.Enums;

namespace Sirius.PaymentAccounts.Dto
{
    [AutoMapTo(typeof(AccountBook))]
    public class CreateAccountBookDto : IShouldNormalize
    {
        public CreateAccountBookDto()
        {
            AccountBookFileUrls = new List<string>();
            Inventories = new List<CreateInventoryDto>();
        }
        public string ProcessDateString{ get; set; }
        public PaymentCategoryType PaymentCategoryType { get; set; }
        public bool IsHousingDue { get; set; }
        public Guid HousingId { get; set; }
        public Guid PaymentCategoryId { get; set; }
        public Guid? FromPaymentAccountId { get; set; }
        public Guid? ToPaymentAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string DocumentDateTimeString { get; set; }
        public string DocumentNumber { get; set; }
        public List<string> AccountBookFileUrls { get; set; }
        
        public bool NettingFromHousingDue { get; set; }
        
        public Guid? HousingIdForNetting { get; set; }
        public Guid? PaymentCategoryIdForNetting { get; set; }
        
        public List<CreateInventoryDto> Inventories { get; set; }

        public void Normalize()
        {
            Amount = Math.Abs(Amount);
        }
    }
}
