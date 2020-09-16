using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace Sirius.HousingCategories.Dto
{
    [AutoMapFrom(typeof(HousingCategory))]
    public class HousingCategoryDto : FullAuditedEntityDto<Guid>
    {
        public string HousingCategoryName { get; set; }
        
    }
}
