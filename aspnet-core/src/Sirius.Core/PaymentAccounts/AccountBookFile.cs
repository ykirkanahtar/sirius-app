using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;

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
            Guid accountBookGuid, long creatorUserId)
        {
            return new()
            {
                Id = id,
                TenantId = tenantId,
                FileUrl = fileUrl,
                AccountBookId = accountBookGuid,
                CreationTime = Clock.Now,
                CreatorUserId = creatorUserId
            };
        }
    }
}