using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace Sirius.Housings
{
    [Table("AppBlocks")]
    public class Block : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        protected Block()
        {
        }

        public virtual int TenantId { get; set; }

        [StringLength(50)] [Required] public string BlockName { get; set; }

        public static async Task<Block> CreateAsync(IBlockPolicy blockPolicy, Guid id, int tenantId, string blockName)
        {
            return await BindEntityAsync(blockPolicy, false, new Block(), id, tenantId, blockName);
        }

        public static async Task<Block> UpdateAsync(IBlockPolicy blockPolicy, Block existingBlock, string blockName)
        {
            return await BindEntityAsync(blockPolicy, true, existingBlock, existingBlock.Id, existingBlock.TenantId,
                blockName);
        }

        private static async Task<Block> BindEntityAsync(IBlockPolicy blockPolicy, bool isUpdate, Block block, Guid id,
            int tenantId, string blockName)
        {
            block ??= new Block();

            block.Id = id;
            block.TenantId = tenantId;
            block.BlockName = blockName;
            
            await blockPolicy.CheckCreateOrUpdateBlockAttemptAsync(block, isUpdate);
            
            return block;
        }
    }
}