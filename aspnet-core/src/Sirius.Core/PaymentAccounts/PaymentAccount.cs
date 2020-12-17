using Abp;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Sirius.Shared.Enums;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.UI;

namespace Sirius.AppPaymentAccounts
{
    [Table("AppPaymentAccounts")]
    public class PaymentAccount : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        protected PaymentAccount()
        {
        }

        public int TenantId { get; set; }

        [Required] [StringLength(50)] public string AccountName { get; private set; }

        public PaymentAccountType PaymentAccountType { get; private set; }
        [DefaultValue(0)] public decimal Balance { get; private set; }
        public string Description { get; private set; }

        public Guid? PersonId { get; private set; }
        public Guid? EmployeeId { get; private set; }


        public string Iban { get; private set; }
        public bool TenantIsOwner { get; private set; }
        public bool IsDefault { get; private set; }

        public static PaymentAccount CreateCashAccount(Guid id, int tenantId, string accountName, string description,
            Guid? personId, Guid? employeeId, bool tenantIsOwner, bool isDefault, decimal? balance = null)
        {
            return BindEntity(new PaymentAccount(), id, tenantId, PaymentAccountType.Cash, accountName, description,
                personId, employeeId,
                tenantIsOwner, isDefault, null, balance);
        }

        public static PaymentAccount CreateBankAccount(Guid id, int tenantId, string accountName, string description,
            string iban, Guid? personId, Guid? employeeId, bool tenantIsOwner, bool isDefault, decimal? balance = null)
        {
            return BindEntity(new PaymentAccount(), id, tenantId, PaymentAccountType.BankAccount, accountName,
                description, personId, employeeId,
                tenantIsOwner, isDefault, iban, balance);
        }

        public static PaymentAccount CreateAdvanceAccount(Guid id, int tenantId, string accountName, string description,
            string iban, Guid? personId, Guid? employeeId, bool tenantIsOwner, bool isDefault, decimal? balance = null)
        {
            return BindEntity(new PaymentAccount(), id, tenantId, PaymentAccountType.AdvanceAccount, accountName,
                description, personId, employeeId,
                tenantIsOwner, isDefault, iban, balance);
        }

        public static PaymentAccount Update(PaymentAccount existingPaymentAccount, string accountName,
            string description, Guid? personId, Guid? employeeId, bool tenantIsOwner, bool isDefault,
            string iban = null, decimal? balance = null)
        {
            return BindEntity(existingPaymentAccount, existingPaymentAccount.Id, existingPaymentAccount.TenantId,
                existingPaymentAccount.PaymentAccountType, accountName, description, personId, employeeId,
                tenantIsOwner, isDefault, iban, balance);
        }

        private static PaymentAccount BindEntity(PaymentAccount paymentAccount, Guid id, int tenantId,
            PaymentAccountType paymentAccountType, string accountName, string description, Guid? personId,
            Guid? employeeId, bool tenantIsOwner, bool isDefault, string iban = null, decimal? balance = null)
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
            paymentAccount.IsDefault = isDefault;
            paymentAccount.Balance = balance ?? 0;

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
            return paymentAccount;
        }

        public static PaymentAccount UnSetDefaultPaymentAccount(PaymentAccount paymentAccount)
        {
            paymentAccount.IsDefault = false;
            return paymentAccount;
        }
    }
}