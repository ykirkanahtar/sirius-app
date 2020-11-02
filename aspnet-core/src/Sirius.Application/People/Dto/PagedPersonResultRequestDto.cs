using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace Sirius.People.Dto
{
    public class PagedPersonResultRequestDto : PagedAndSortedResultRequestDto
    {
        public PagedPersonResultRequestDto()
        {
            HousingIds = new List<Guid>();
        }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public List<Guid> HousingIds { get; set; }
    }
}