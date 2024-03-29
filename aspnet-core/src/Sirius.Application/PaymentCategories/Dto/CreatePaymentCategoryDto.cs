﻿using System;
using System.ComponentModel.DataAnnotations;
using Abp.AutoMapper;
using Sirius.Housings;
using Sirius.Shared.Enums;

namespace Sirius.PaymentCategories.Dto
{
    [AutoMapTo(typeof(PaymentCategory))]

    public class CreatePaymentCategoryDto
    {
        [StringLength(50)]
        public string PaymentCategoryName { get; set; }
        public bool IsHousingDue { get; set; }
        public bool IsValidForAllPeriods { get; set; }
        public Guid? DefaultFromPaymentAccountId { get; set; }
        public Guid? DefaultToPaymentAccountId { get; set; }
    }
}
