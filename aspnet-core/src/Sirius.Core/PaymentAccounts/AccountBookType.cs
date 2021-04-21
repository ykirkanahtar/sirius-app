namespace Sirius.PaymentAccounts
{
    public enum AccountBookType
    {
        HousingDue = 1,
        OtherPaymentWithNettingForHousingDue = 2,
        FirstTransferForPaymentAccount = 3,
        Other = 4,
        TransferForPaymentAccountToNextPeriod = 5,
        TransferForPaymentAccountFromPreviousPeriod = 6
    }
}