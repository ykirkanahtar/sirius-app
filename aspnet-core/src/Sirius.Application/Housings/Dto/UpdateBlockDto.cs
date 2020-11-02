using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Abp.Extensions;
using Abp.Runtime.Validation;

namespace Sirius.Housings.Dto
{
    public class UpdateBlockDto : IEntityDto<Guid>, ICustomValidate
    {
        public Guid Id { get; set; }
        public string BlockName { get; set; }

        public void AddValidationErrors(CustomValidationContext context)
        {
            if (BlockName.IsNullOrWhiteSpace())
            {
                context.Results.Add(new ValidationResult("Blok değeri dolu olmalı", new[] {nameof(Block)}));
            }
        }
    }
}