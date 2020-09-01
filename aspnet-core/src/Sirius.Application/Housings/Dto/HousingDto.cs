using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace Sirius.Housings.Dto
{
    [AutoMapFrom(typeof(Housing))]
    public class HousingDto : FullAuditedEntityDto<Guid>
    {
        public string Block { get; set; }
        public string Apartment { get; set; }
    }
}
