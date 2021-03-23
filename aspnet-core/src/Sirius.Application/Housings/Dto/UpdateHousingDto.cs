using System;
using Abp.Application.Services.Dto;
using Sirius.HousingPaymentPlans.Dto;
using Sirius.Shared.Enums;

namespace Sirius.Housings.Dto
{
    public class UpdateHousingDto : IEntityDto<Guid>
    {
        public Guid Id { get; set; }
        public Guid BlockId { get; set; }
        public string Apartment { get; set; }
        public Guid HousingCategoryId { get; set; }
        public bool TenantIsResiding { get; set; }
        public bool DeleteTransferForHousingDue { get; set; }
        public ResidentOrOwner TransferIsForResidentOrOwner { get; set; }
        public decimal? TransferAmount { get; set; }
        public bool TransferIsDebt { get; set; }
        public DateTime TransferDate { get; set; }
        public string TransferDescription { get; set; }
    }
}