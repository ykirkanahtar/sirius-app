using System;
using Abp.Application.Services.Dto;

namespace Sirius.Housings.Dto
{
    public class HousingForListDto: FullAuditedEntityDto<Guid>
    {
        public Guid Id { get; set; }
        public string Block { get; set; }
        public string Apartment { get; set; }
        public string HousingCategoryName { get; set; }
        public bool TenantIsResiding { get; set; }
        public decimal Balance { get; set; }
    }
}