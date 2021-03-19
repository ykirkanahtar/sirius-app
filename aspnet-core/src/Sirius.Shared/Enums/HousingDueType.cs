namespace Sirius.Shared.Enums
{
    // public enum HousingDueType
    // {
    //     RegularHousingDue = 1,
    //     TransferForRegularHousingDue = 2,
    //     Netting = 3,
    //     AdditionalHousingDueForResident = 4,
    //     AdditionalHousingDueForOwner = 5,
    //     TransferForAdditionalHousingDue = 6,
    // }

    public enum HousingPaymentPlanType
    {
        HousingDueDefinition = 1,
        HousingDuePayment = 2,
        Transfer = 2,
        Netting = 3
    }
}