﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.UI;
using Sirius.Shared.Enums;

namespace Sirius.PaymentAccounts
{
    [Table("AppPaymentAccounts")]
    public class PaymentAccount : FullAuditedEntity<Guid>, IMustHaveTenant, IPassivable
    {
        protected PaymentAccount()
        {
        }

        public int TenantId { get; set; }

        [Required] [StringLength(50)] public string AccountName { get; private set; }

        public PaymentAccountType PaymentAccountType { get; private set; }
        [DefaultValue(0)] public decimal Balance { get; private set; }
        public string Description { get; private set; }

        public DateTime? FirstTransferDateTime { get; private set; } //ilk devir hareketi

        public Guid? PersonId { get; private set; }
        public Guid? EmployeeId { get; private set; }


        public string Iban { get; private set; }
        public bool TenantIsOwner { get; private set; }
        public bool AllowNegativeBalance { get; private set; }
        public bool IsActive { get; set; }
        
        public static PaymentAccount CreateCashAccount(Guid id, int tenantId, string accountName, string description,
            Guid? personId, Guid? employeeId, bool tenantIsOwner,  bool allowNegativeBalance,
            decimal? balance = null,
            DateTime? firstTransferDateTime = null)
        {
            return BindEntity(new PaymentAccount(), id, tenantId, PaymentAccountType.Cash, accountName, description,
                personId, employeeId,
                tenantIsOwner, allowNegativeBalance, true, null, balance, firstTransferDateTime);
        }

        public static PaymentAccount CreateBankAccount(Guid id, int tenantId, string accountName, string description,
            string iban, Guid? personId, Guid? employeeId, bool tenantIsOwner,
            bool allowNegativeBalance, decimal? balance = null,
            DateTime? firstTransferDateTime = null)
        {
            return BindEntity(new PaymentAccount(), id, tenantId, PaymentAccountType.BankAccount, accountName,
                description, personId, employeeId,
                tenantIsOwner, allowNegativeBalance, true, iban, balance, firstTransferDateTime);
        }

        public static PaymentAccount Update(PaymentAccount existingPaymentAccount, string accountName,
            string description, Guid? personId, Guid? employeeId, bool tenantIsOwner,
            decimal balance, bool allowNegativeBalance, bool isActive, string iban = null, DateTime? firstTransferDateTime = null)
        {
            return BindEntity(existingPaymentAccount, existingPaymentAccount.Id, existingPaymentAccount.TenantId,
                existingPaymentAccount.PaymentAccountType, accountName, description, personId, employeeId,
                tenantIsOwner, allowNegativeBalance, isActive, iban, balance, firstTransferDateTime);
        }

        private static PaymentAccount BindEntity(PaymentAccount paymentAccount, Guid id, int tenantId,
            PaymentAccountType paymentAccountType, string accountName, string description, Guid? personId,
            Guid? employeeId, bool tenantIsOwner, bool allowNegativeBalance, bool isActive, string iban = null,
            decimal? balance = null,
            DateTime? firstTransferDateTime = null)
        {
            paymentAccount ??= new PaymentAccount();


            paymentAccount.Id = id;
            paymentAccount.TenantId = tenantId;
            paymentAccount.AccountName = accountName;
            paymentAccount.PersonId = personId;
            paymentAccount.EmployeeId = employeeId;
            paymentAccount.Description = description;
            paymentAccount.PaymentAccountType = paymentAccountType;
            paymentAccount.Iban = iban;
            paymentAccount.TenantIsOwner = tenantIsOwner;
            paymentAccount.Balance = balance ?? 0;
            paymentAccount.FirstTransferDateTime = firstTransferDateTime.HasValue
                ? firstTransferDateTime.Value.Date + new TimeSpan(0, 0, 0)
                : null;
            paymentAccount.AllowNegativeBalance = allowNegativeBalance;
            paymentAccount.IsActive = isActive;

            return paymentAccount;
        }

        public static PaymentAccount IncreaseBalance(PaymentAccount paymentAccount, decimal amount)
        {
            if (amount < 0)
            {
                throw new UserFriendlyException("Tutar sıfırdan küçük olamaz");
            }

            paymentAccount.Balance += amount;
            return paymentAccount;
        }

        public static PaymentAccount DecreaseBalance(PaymentAccount paymentAccount, decimal amount)
        {
            if (amount < 0)
            {
                throw new UserFriendlyException("Tutar sıfırdan küçük olamaz");
            }

            paymentAccount.Balance -= amount;

            if (paymentAccount.PaymentAccountType == PaymentAccountType.BankAccount && paymentAccount.Balance < 0)
            {
                throw new UserFriendlyException("Bakiye sıfırdan küçük olamaz");
            }

            return paymentAccount;
        }

        public void SetBalance(decimal amount)
        {
            if (AllowNegativeBalance == false && amount < 0)
            {
                throw new UserFriendlyException("Tutar sıfırdan küçük olamaz");
            }

            Balance = amount;
        }

    }
}