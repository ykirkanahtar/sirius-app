using System;
using Abp.AutoMapper;

namespace Sirius.Housings.Dto
{
    [AutoMapTo(typeof(HousingPerson))]

    public class AddHousingPersonDto
    {
        public Guid HousingId { get; set; }
        public Guid PersonId { get; set; }
    }
}