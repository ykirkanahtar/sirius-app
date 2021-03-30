using System;
using Abp.AutoMapper;
using Abp.Runtime.Validation;
using Sirius.Shared.Enums;

namespace Sirius.HousingPaymentPlans.Dto
{
    [AutoMapTo(typeof(HousingPaymentPlan))]
    public class CreateOrUpdateTransferForHousingDueDto : IShouldNormalize
    {
        public Guid HousingId { get; set; }
        public ResidentOrOwner ResidentOrOwner { get; set; }
        public decimal? Amount { get; set; }
        public bool IsDebt { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }

        public void Normalize()
        {
            Amount = Math.Abs(Amount.GetValueOrDefault());
        }
    }
}