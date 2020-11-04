using System.Threading.Tasks;
using Abp.Domain.Services;

namespace Sirius.Housings
{
    public interface IHousingPolicy : IDomainService
    {
        Task CheckCreateOrUpdateHousingAttemptAsync(Housing housing, bool isUpdate);
    }
}