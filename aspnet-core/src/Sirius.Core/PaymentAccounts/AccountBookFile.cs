using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace Sirius.PaymentAccounts
{
    [Table("AppAccountBookFiles")]
    public class AccountBookFile : Entity<Guid>, IFullAudited
    {
        protected AccountBookFile()
        {
            
        }

        public int TenantId { get; set; }

        [Required] [StringLength(200)] public string FileUrl { get; private set; }

        [Required] public virtual Guid AccountBookId { get; private set; }

        public DateTime CreationTime { get; set; }
        public long? CreatorUserId { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public long? LastModifierUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletionTime { get; set; }
        public long? DeleterUserId { get; set; }

        public static AccountBookFile Create(Guid id, int tenantId, string fileUrl,
            Guid accountBookGuid)
        {
            var accountBookFile = new AccountBookFile();
            accountBookFile.Id = id;
            accountBookFile.TenantId = tenantId;
            accountBookFile.FileUrl = fileUrl;
            accountBookFile.AccountBookId = accountBookGuid;

            return accountBookFile;
        }
    }
}