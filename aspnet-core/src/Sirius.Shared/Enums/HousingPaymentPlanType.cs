namespace Sirius.Shared.Enums
{
    public enum HousingPaymentPlanType
    {
        HousingDueDefinition = 1,
        HousingDuePayment = 2,
        Transfer = 3,
        Netting = 4,
        TransferForHousingDuePaymentToNextPeriod = 5,
        TransferForHousingDuePaymentFromPreviousPeriod = 6
    }
}