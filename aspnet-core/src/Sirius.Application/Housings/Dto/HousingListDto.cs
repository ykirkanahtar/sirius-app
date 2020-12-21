using System;
using Abp.Application.Services.Dto;

namespace Sirius.Housings.Dto
{
    public class HousingListDto: FullAuditedEntityDto<Guid>
    {
        public string Block { get; set; }
        public string Apartment { get; set; }
        public string HousingCategoryName { get; set; }
        public bool TenantIsResiding { get; set; }
        public decimal Balance { get; set; }
    }
}