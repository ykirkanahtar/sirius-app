using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Abp.Extensions;
using Abp.Runtime.Validation;

namespace Sirius.Housings.Dto
{
    public class UpdateHousingDto : IEntityDto<Guid>, ICustomValidate
    {
        public Guid Id { get; set; }
        public Guid? BlockId { get; set; }
        public string Apartment { get; set; }
        public Guid HousingCategoryId { get; set; }

        public void AddValidationErrors(CustomValidationContext context)
        {
            if (!BlockId.HasValue && Apartment.IsNullOrWhiteSpace())
            {
                context.Results.Add(new ValidationResult("Blok ya da daire değerlerinden en az bir tanesi dolu olmalı",
                    new[] {"Apartment", "Block"}));
            }
        }
    }
}