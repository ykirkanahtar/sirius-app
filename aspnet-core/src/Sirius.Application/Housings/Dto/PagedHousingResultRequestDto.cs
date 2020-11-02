using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace Sirius.Housings.Dto
{
    public class PagedHousingResultRequestDto : PagedAndSortedResultRequestDto
    {
        public PagedHousingResultRequestDto()
        {
            HousingIds = new List<Guid>();
            HousingCategoryIds = new List<Guid>();
            PersonIds = new List<Guid>();
        }
        public List<Guid> HousingIds { get; set; }
        public List<Guid> HousingCategoryIds { get; set; }
        public List<Guid> PersonIds { get; set; }
    }
}