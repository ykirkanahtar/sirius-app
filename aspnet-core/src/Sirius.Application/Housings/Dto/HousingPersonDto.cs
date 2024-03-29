using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sirius.People.Dto;
using Sirius.Shared.Enums;

namespace Sirius.Housings.Dto
{
    [AutoMapFrom(typeof(HousingPerson))]
    public class HousingPersonDto : FullAuditedEntityDto<Guid>
    {
        public Guid HousingId { get; set; }
        public Guid PersonId { get; set; }
        public bool IsTenant { get; set; }
        public bool Contact { get; set; }

        public virtual PersonDto Person { get; set; }
        public virtual HousingDto Housing { get; set; }
    }
}