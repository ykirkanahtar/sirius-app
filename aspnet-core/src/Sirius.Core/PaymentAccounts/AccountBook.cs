using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.UI;
using JetBrains.Annotations;

namespace Sirius.PaymentAccounts
{
    [Table("AppAccountBooks")]
    public class AccountBook : AggregateRoot<Guid>, IFullAudited, IMustHaveTenant
    {
        protected AccountBook()
        {
            AccountBookFiles = new List<AccountBookFile>();
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
        public bool EncashmentHousing { get; private set; }
        public Guid? HousingIdForEncachment { get; private set; }
        public DateTime? DocumentDateTime { get; private set; }
        public string DocumentNumber { get; private set; }
        public decimal? FromPaymentAccountCurrentBalance { get; private set; }
        public decimal? ToPaymentAccountCurrentBalance { get; private set; }
        public AccountBookType AccountBookType { get; private set; }
        public DateTime CreationTime { get; set; }
        public long? CreatorUserId { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public long? LastModifierUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletionTime { get; set; }
        public long? DeleterUserId { get; set; }

        public virtual ICollection<AccountBookFile> AccountBookFiles { get; private set; }

        public static AccountBook ShallowCopy(AccountBook accountBook)
        {
            return (AccountBook) accountBook.MemberwiseClone();
        }

        private static async Task<AccountBook> BindEntityAsync(
            IAccountBookPolicy accountBookPolicy,
            bool isUpdate,
            bool isTransferForPaymentAccount,
            Guid id,
            int tenantId,
            AccountBookType accountBookType,
            DateTime processDateTime,
            Guid paymentCategoryId,
            Guid? housingId,
            bool encashmentHousing,
            Guid? housingIdForEncachment,
            [CanBeNull] PaymentAccount fromPaymentAccount,
            [CanBeNull] PaymentAccount toPaymentAccount,
            decimal amount,
            string description,
            DateTime? documentDateTime,
            string documentNumber,
            ICollection<AccountBookFile> accountBookFiles,
            DateTime? creationTime,
            long? creatorUserId,
            DateTime? modificationDateTime,
            long? modifierUserId,
            [CanBeNull] AccountBook existingAccountBook = null
        )
        {
            var accountBook = existingAccountBook ?? new AccountBook();

            accountBook.Id = id;
            accountBook.TenantId = tenantId;
            accountBook.AccountBookType = accountBookType;
            accountBook.ProcessDateTime = processDateTime;
            accountBook.DocumentDateTime = documentDateTime;
            accountBook.PaymentCategoryId = paymentCategoryId;
            accountBook.Description = description;
            accountBook.DocumentNumber = documentNumber;
            accountBook.EncashmentHousing = encashmentHousing;
            accountBook.HousingIdForEncachment = housingIdForEncachment;
            accountBook.HousingId = housingId;
            accountBook.Amount = amount;
            accountBook.FromPaymentAccountId = fromPaymentAccount?.Id;
            accountBook.ToPaymentAccountId = toPaymentAccount?.Id;
            accountBook.AccountBookFiles = accountBookFiles;

            if (creationTime.HasValue)
            {
                accountBook.CreationTime = creationTime.Value;
            }

            if (creatorUserId.HasValue)
            {
                accountBook.CreatorUserId = creatorUserId.Value;
            }

            if (modificationDateTime.HasValue)
            {
                accountBook.LastModificationTime = modificationDateTime.Value;
            }

            if (modifierUserId.HasValue)
            {
                accountBook.LastModifierUserId = modifierUserId.Value;
            }

            if (isTransferForPaymentAccount)
            {
                await accountBookPolicy.CheckForTransferForPaymentAccountAsync(accountBook, toPaymentAccount);
            }

            accountBookPolicy.CheckCreateOrUpdateAttempt(accountBook, fromPaymentAccount, toPaymentAccount, isUpdate);

            return accountBook;
        }

        public static async Task<AccountBook> UpdateAsync(
            IAccountBookPolicy accountBookPolicy,
            AccountBook existingAccountBook,
            DateTime processDateTime,
            [CanBeNull] PaymentAccount fromPaymentAccount,
            [CanBeNull] PaymentAccount toPaymentAccount,
            decimal amount,
            string description,
            DateTime? documentDateTime,
            string documentNumber,
            ICollection<AccountBookFile> accountBookFiles,
            long modifierUserId)
        {
            return await BindEntityAsync(accountBookPolicy, true, false, existingAccountBook.Id,
                existingAccountBook.TenantId,
                existingAccountBook.AccountBookType,
                processDateTime,
                existingAccountBook.PaymentCategoryId,
                existingAccountBook.HousingId,
                existingAccountBook.EncashmentHousing,
                existingAccountBook.HousingIdForEncachment,
                fromPaymentAccount,
                toPaymentAccount,
                amount, description, documentDateTime, documentNumber, accountBookFiles, null, null,
                DateTime.UtcNow, modifierUserId, existingAccountBook);
        }

        public static async Task<AccountBook> CreateHousingDueAsync(
            IAccountBookPolicy accountBookPolicy,
            Guid id,
            int tenantId,
            DateTime processDateTime,
            Guid paymentCategoryId,
            Guid housingId,
            PaymentAccount toPaymentAccountId,
            decimal amount,
            string description,
            ICollection<AccountBookFile> accountBookFiles,
            long creatorUserId)
        {
            return await BindEntityAsync(accountBookPolicy, false, false, id, tenantId,
                AccountBookType.HousingDue,
                processDateTime,
                paymentCategoryId, housingId, false, null, null,
                toPaymentAccountId, amount, description, null, null, accountBookFiles, DateTime.UtcNow, creatorUserId,
                null, null);
        }

        public static async Task<AccountBook> CreateForPaymentAccountTransferAsync( //Ödeme hesabı devir hareketi için
            IAccountBookPolicy accountBookPolicy,
            Guid id,
            int tenantId,
            DateTime processDateTime,
            Guid paymentCategoryId,
            PaymentAccount toPaymentAccount,
            decimal amount,
            string description,
            ICollection<AccountBookFile> accountBookFiles,
            long creatorUserId)
        {
            return await BindEntityAsync(accountBookPolicy, false, true, id, tenantId,
                AccountBookType.ForPaymentAccount,
                processDateTime,
                paymentCategoryId, null, false, null, null,
                toPaymentAccount, amount, description, null, null, accountBookFiles, DateTime.UtcNow, creatorUserId,
                null, null);
        }

        public static async Task<AccountBook> CreateAsync(
            IAccountBookPolicy accountBookPolicy,
            Guid id,
            int tenantId,
            AccountBookType accountBookType,
            DateTime processDateTime,
            Guid paymentCategoryId,
            Guid? housingId,
            bool encashmentHousing,
            Guid? housingIdForEncachment,
            [CanBeNull] PaymentAccount fromPaymentAccount,
            [CanBeNull] PaymentAccount toPaymentAccount,
            decimal amount,
            string description,
            DateTime? documentDateTime,
            string documentNumber,
            ICollection<AccountBookFile> accountBookFiles,
            long creatorUserId)
        {
            return await BindEntityAsync(accountBookPolicy, false, false, id, tenantId,
                accountBookType,
                processDateTime,
                paymentCategoryId, housingId, encashmentHousing, housingIdForEncachment,
                fromPaymentAccount,
                toPaymentAccount, amount, description, documentDateTime, documentNumber, accountBookFiles,
                DateTime.UtcNow, creatorUserId, null, null);
        }

        public void SetToPaymentAccountCurrentBalance(decimal balance)
        {
            if (balance < 0)
            {
                throw new UserFriendlyException("Bakiye eksiye düşemez");
            }

            ToPaymentAccountCurrentBalance = balance;
        }

        public void SetFromPaymentAccountCurrentBalance(decimal balance)
        {
            if (balance < 0)
            {
                throw new UserFriendlyException("Bakiye eksiye düşemez");
            }

            FromPaymentAccountCurrentBalance = balance;
        }
    }
}