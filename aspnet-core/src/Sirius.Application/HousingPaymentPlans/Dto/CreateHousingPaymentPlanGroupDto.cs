using System;
using System.Collections.Generic;
using Castle.Components.DictionaryAdapter;
using Sirius.Shared.Enums;

namespace Sirius.HousingPaymentPlans.Dto
{
    public class CreateHousingPaymentPlanGroupDto
    {
        public CreateHousingPaymentPlanGroupDto()
        {
            HousingPaymentPlanGroupForHousings = new List<HousingPaymentPlanGroupForHousingDto>();
            HousingPaymentPlanGroupForHousingCategories =
                new EditableList<HousingPaymentPlanGroupForHousingCategoryDto>();
        }
        public string HousingPaymentPlanGroupName { get; set; }
        public int CountOfMonth { get; set; }
        public Guid DefaultToPaymentAccountId { get; set; }
        public int PaymentDayOfMonth { get; set; }
        public string StartDateString { get; set; }
        public string Description { get; set; }
        public ResidentOrOwner ResidentOrOwner { get; set; }

        public List<HousingPaymentPlanGroupForHousingDto> HousingPaymentPlanGroupForHousings { get; set; }

        public List<HousingPaymentPlanGroupForHousingCategoryDto> HousingPaymentPlanGroupForHousingCategories
        {
            get;
            set;
        }
    }
}