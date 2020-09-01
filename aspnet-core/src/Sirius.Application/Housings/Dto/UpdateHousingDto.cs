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
        public string Block { get; set; }
        public string Apartment { get; set; }

        public void AddValidationErrors(CustomValidationContext context)
        {
            if (Block.IsNullOrWhiteSpace() && Apartment.IsNullOrWhiteSpace())
            {
                context.Results.Add(new ValidationResult("Blok ya da daire değerlerinden en az bir tanesi dolu olmalı", new[] { "Apartment", "Block" }));
            }
        }
    }
}
