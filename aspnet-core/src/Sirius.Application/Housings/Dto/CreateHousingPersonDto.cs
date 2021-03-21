using System;
using System.Collections.Generic;
using Abp.AutoMapper;
using Sirius.Shared.Enums;

namespace Sirius.Housings.Dto
{
    [AutoMapTo(typeof(Housing))]

    public class CreateHousingPersonDto
    {
        public CreateHousingPersonDto()
        {
            PeopleIds = new List<Guid>();
        }
        public Guid HousingId { get; set; }
        public List<Guid> PeopleIds { get; set; }
        public bool IsTenant { get;  set; }
        public bool Contact { get; set; }
    }
}