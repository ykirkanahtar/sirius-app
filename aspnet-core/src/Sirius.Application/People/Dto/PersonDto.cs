using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace Sirius.People.Dto
{
    [AutoMapFrom(typeof(Person))]
    public class PersonDto : FullAuditedEntityDto<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; } //FirstName + LastName
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
    }
}
