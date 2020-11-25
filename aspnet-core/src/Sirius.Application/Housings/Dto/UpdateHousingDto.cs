using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Abp.Extensions;
using Abp.Runtime.Validation;

namespace Sirius.Housings.Dto
{
    public class UpdateHousingDto : IEntityDto<Guid>
    {
        public Guid Id { get; set; }
        public Guid BlockId { get; set; }
        public string Apartment { get; set; }
        public Guid HousingCategoryId { get; set; }
        public bool TenantIsResiding { get; set; }

    }
}