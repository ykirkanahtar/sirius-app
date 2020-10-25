using System;
using Abp.AutoMapper;
using Sirius.Shared.Enums;

namespace Sirius.Housings.Dto
{
    [AutoMapTo(typeof(Housing))]

    public class CreateHousingPersonDto
    {
        public Guid HousingId { get; set; }
        public Guid PersonId { get; set; }
        public bool IsTenant { get;  set; }
        public bool Contact { get; set; }
    }
}