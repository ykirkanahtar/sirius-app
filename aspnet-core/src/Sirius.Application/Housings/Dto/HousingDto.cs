﻿using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using JetBrains.Annotations;
using Sirius.HousingPaymentPlans;
using Sirius.HousingPaymentPlans.Dto;

namespace Sirius.Housings.Dto
{
    [AutoMapFrom(typeof(Housing))]
    public class HousingDto : FullAuditedEntityDto<Guid>
    {
        public Guid BlockId { get; set; }
        public string Apartment { get; set; }
        public Guid HousingCategoryId { get; set; }
        public decimal Balance { get; set; }
        public decimal ResidentBalance { get; set; }
        public decimal OwnerBalance { get; set; }
        public bool TenantIsResiding { get; set; }
        [CanBeNull] public HousingPaymentPlanDto FirstHousingDueTransferPaymentPlan { get; set; }

        public virtual Block Block { get; set; }
        public virtual HousingCategoryDto HousingCategory { get; set; }
        public virtual HousingPersonDto HousingPerson { get; set; }
    }
}
