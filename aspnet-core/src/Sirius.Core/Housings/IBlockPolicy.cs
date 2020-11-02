using System.Threading.Tasks;
using Abp.Domain.Services;

namespace Sirius.Housings
{
    public interface IBlockPolicy : IDomainService
    {
        Task CheckCreateOrUpdateBlockAttemptAsync(Block block, bool isUpdate);
    }
}