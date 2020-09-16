using System;
using Abp.Application.Services.Dto;

namespace Sirius.HousingCategories.Dto
{
    public class UpdateHousingCategoryDto : IEntityDto<Guid>
    {
        public Guid Id { get; set; }
        public string HousingCategoryName { get; set; }
    }
}
