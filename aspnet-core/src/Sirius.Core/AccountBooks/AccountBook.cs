using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Sirius.Shared.Enums;

namespace Sirius.AccountBooks
{
    [Table("AppAccountBookss")]
    public class AccountBook : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        protected AccountBook()
        {

        }

        public int TenantId { get; set; }
        public DateTime ProcessDateTime { get; private set; }
        // public AccountBookProcessType AccountBookProcessType { get; private set; }
        
        public Guid PaymentCategoryId { get; private set; }
        public Guid? HousingId { get; private set; }
        public Guid? FromPaymentAccountId { get; private set; }
        public Guid? ToPaymentAccountId { get; private set; }
        public decimal Amount { get; private set; }
        public string Description { get; private set; }
        public DateTime? DocumentDateTime { get; private set; }
        public string DocumentNumber { get; private set; }

        private static AccountBook BindEntity(AccountBook accountBook, Guid id, int tenantId, DateTime processDateTime, Guid paymentCategoryId, Guid? housingId, Guid? fromPaymentAccountId, Guid? toPaymentAccountId, decimal amount, string description, DateTime? documentDateTime, string documentNumber)
        {
            accountBook ??= new AccountBook();
            
            accountBook.Id = id;
            accountBook.TenantId = tenantId;
            accountBook.ProcessDateTime = processDateTime;
            accountBook.DocumentDateTime = documentDateTime;
            accountBook.PaymentCategoryId = paymentCategoryId;
            accountBook.Description = description;
            accountBook.DocumentNumber = documentNumber;
            accountBook.HousingId = housingId;
            accountBook.Amount = amount;
            accountBook.FromPaymentAccountId = fromPaymentAccountId;
            accountBook.ToPaymentAccountId = toPaymentAccountId;

            return accountBook;
        }

        public static AccountBook Update(AccountBook existingAccountBook, string description,
            DateTime? documentDateTime, string documentNumber)
        {
            
            return BindEntity(existingAccountBook, existingAccountBook.Id, existingAccountBook.TenantId, existingAccountBook.ProcessDateTime,
                existingAccountBook.PaymentCategoryId, existingAccountBook.HousingId,
                existingAccountBook.FromPaymentAccountId, existingAccountBook.ToPaymentAccountId,
                existingAccountBook.Amount, description, documentDateTime, documentNumber);
        }
        
        public static AccountBook CreateHousingDue(Guid id, int tenantId, DateTime processDateTime,Guid paymentCategoryId, Guid housingId, Guid paymentAccountId, decimal amount, string description)
        {
            return BindEntity(new AccountBook(), id, tenantId, processDateTime, paymentCategoryId, housingId, null,
                paymentAccountId, amount, description, null, null);
        }

        public static AccountBook Create(Guid id, int tenantId, DateTime processDateTime,Guid paymentCategoryId, Guid? housingId, Guid? fromPaymentAccountId, Guid? toPaymentAccountId, decimal amount, string description, DateTime? documentDateTime, string documentNumber)
        {

            return BindEntity(new AccountBook(),  id, tenantId, processDateTime, paymentCategoryId, housingId, fromPaymentAccountId,
                toPaymentAccountId, amount, description, documentDateTime, documentNumber);
        }
        // public static AccountBook CreateRefundHousingDue(Guid id, int tenantId, DateTime processDateTime, Guid housingId, Guid paymentAccountId, decimal amount, string description)
        // {
        //     return BindEntity(new AccountBook(), id, tenantId, processDateTime, AccountBookProcessType.HousingDue, housingId, paymentAccountId,
        //         null, amount, description, null, null);
        // }
        //
        // public static AccountBook CreateBillPayment(Guid id, int tenantId, DateTime processDateTime, Guid paymentAccountId, decimal amount, string description, DateTime? documentDateTime, string documentNumber)
        // {
        //     return BindEntity(new AccountBook(), id, tenantId, processDateTime, AccountBookProcessType.BillPayment, null, paymentAccountId,
        //         null, amount, description, documentDateTime, documentNumber);
        // }
        //
        // public static AccountBook CreateTransferFromPreviousPeriod(Guid id, int tenantId, DateTime processDateTime, Guid paymentAccountId, decimal amount, string description)
        // {
        //     return BindEntity(new AccountBook(), id, tenantId, processDateTime, AccountBookProcessType.TransferFromThePreviousPeriod, null,
        //         null, paymentAccountId, amount, description, null, null);
        // }
        //
        // public static AccountBook CreateSalaryPayment(Guid id, int tenantId, DateTime processDateTime, Guid fromPaymentAccountId, Guid toPaymentAccountId,  decimal amount, string description)
        // {
        //     return BindEntity(new AccountBook(), id, tenantId, processDateTime, AccountBookProcessType.SalaryPayment, null,
        //         fromPaymentAccountId, toPaymentAccountId, amount, description, null, null);
        // }
        //
        // public static AccountBook CreateBankTransferFee(Guid id, int tenantId, DateTime processDateTime, Guid fromPaymentAccountId, decimal amount, string description)
        // {
        //     return BindEntity(new AccountBook(), id, tenantId, processDateTime, AccountBookProcessType.BankTransferFee, null,
        //         fromPaymentAccountId, null, amount, description, null, null);
        // }
        //
        // public static AccountBook CreateEftFee(Guid id, int tenantId, DateTime processDateTime, Guid fromPaymentAccountId, decimal amount, string description)
        // {
        //     return BindEntity(new AccountBook(), id, tenantId, processDateTime, AccountBookProcessType.EftFee, null, fromPaymentAccountId,
        //         null, amount, description, null, null);
        // }
        //
        // public static AccountBook CreateBankingAndIssuranceTransactionTax(Guid id, int tenantId, DateTime processDateTime, Guid fromPaymentAccountId, decimal amount, string description)
        // {
        //     return BindEntity(new AccountBook(), id, tenantId, processDateTime, AccountBookProcessType.BankingAndInsuranceTransactionTax, null,
        //         fromPaymentAccountId, null, amount, description, null, null);
        // }
        //
        // public static AccountBook CreateWorkerWarmingFeePayment(Guid id, int tenantId, DateTime processDateTime, Guid fromPaymentAccountId, Guid toPaymentAccountId, decimal amount, string description)
        // {
        //     return BindEntity(new AccountBook(), id, tenantId, processDateTime, AccountBookProcessType.WorkerWarmingFee, null,
        //         fromPaymentAccountId, toPaymentAccountId, amount, description, null, null);
        // }
        //
        // public static AccountBook CreateBonusPayment(Guid id, int tenantId, DateTime processDateTime, Guid fromPaymentAccountId, Guid toPaymentAccountId, decimal amount, string description)
        // {
        //     return BindEntity(new AccountBook(), id, tenantId, processDateTime, AccountBookProcessType.BonusPayment, null,
        //         fromPaymentAccountId, toPaymentAccountId, amount, description, null, null);
        // }
        //
        // public static AccountBook CreateTransferToAdvanceAccountPayment(Guid id, int tenantId, DateTime processDateTime, Guid fromPaymentAccountId, Guid toPaymentAccountId, decimal amount, string description)
        // {
        //     return BindEntity(new AccountBook(), id, tenantId, processDateTime, AccountBookProcessType.TransferToAdvanceAccount, null,
        //         fromPaymentAccountId, toPaymentAccountId, amount, description, null, null);
        // }
        //
        // public static AccountBook CreateOtherPayment(Guid id, int tenantId, DateTime processDateTime, Guid? housingId, Guid? fromPaymentAccountId, Guid? toPaymentAccountId, decimal amount, string description, DateTime? documentDateTime, string documentNumber)
        // {
        //
        //     return BindEntity(new AccountBook(),  id, tenantId, processDateTime, AccountBookProcessType.Other, housingId, fromPaymentAccountId,
        //         toPaymentAccountId, amount, description, documentDateTime, documentNumber);
        // }
        
    }
}
