using System;
using Abp.Application.Services.Dto;

namespace Sirius.Housings.Dto
{
    public class PagedHousingPersonResultRequestDto : PagedAndSortedResultRequestDto
    {
        public Guid HousingId { get; set; }
    }
}