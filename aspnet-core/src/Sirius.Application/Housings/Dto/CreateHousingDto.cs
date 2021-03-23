using System;
using Abp.AutoMapper;
using Sirius.HousingPaymentPlans.Dto;

namespace Sirius.Housings.Dto
{
    [AutoMapTo(typeof(Housing))]

    public class CreateHousingDto
    {
        public CreateHousingDto()
        {
            TransferForHousingDue = new CreateOrUpdateTransferForHousingDueDto();
        }
        
        public Guid BlockId { get; set; }
        public string Apartment { get; set; }
        public Guid HousingCategoryId { get; set; }
        public bool TenantIsResiding { get; set; }
        public CreateOrUpdateTransferForHousingDueDto TransferForHousingDue { get; set; }
    }
}
