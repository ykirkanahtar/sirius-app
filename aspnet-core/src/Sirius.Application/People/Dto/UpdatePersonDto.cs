using System;
using Abp.Application.Services.Dto;

namespace Sirius.People.Dto
{
    public class UpdatePersonDto : IEntityDto<Guid>
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
    }
}
