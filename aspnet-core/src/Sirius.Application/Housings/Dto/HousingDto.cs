using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sirius.HousingCategories.Dto;

namespace Sirius.Housings.Dto
{
    [AutoMapFrom(typeof(Housing))]
    public class HousingDto : FullAuditedEntityDto<Guid>
    {
        public string Block { get; set; }
        public string Apartment { get; set; }
        public Guid HousingCategoryId { get; set; }
        
        public virtual HousingCategoryDto HousingCategory { get; set; }
    }
}
