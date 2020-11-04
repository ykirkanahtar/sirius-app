using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;

namespace Sirius.Housings
{
    public class HousingPolicy : IHousingPolicy
    {
        private readonly IRepository<Housing, Guid> _housingRepository;
        private readonly IRepository<Block, Guid> _blockRepository;

        public HousingPolicy(IRepository<Housing, Guid> housingRepository, IRepository<Block, Guid> blockRepository)
        {
            _housingRepository = housingRepository;
            _blockRepository = blockRepository;
        }

        public async Task CheckCreateOrUpdateHousingAttemptAsync(Housing housing, bool isUpdate)
        {
            if (housing == null)
            {
                throw new ArgumentNullException(nameof(Housing));
            }

            if (housing.Apartment.IsNullOrWhiteSpace())
            {
                throw new UserFriendlyException("Daire alanı dolu olmalıdır.");
            }
            
            var block = await _blockRepository.GetAsync(housing.BlockId);

            var sameHousingQuery =
                _housingRepository.GetAll().Include(p => p.Block)
                    .Where(p =>
                        p.BlockId == housing.BlockId &&
                        string.Equals(p.Apartment.ToLower(), housing.Apartment.ToLowerInvariant()))
                    .WhereIf(isUpdate, p => p.Id != housing.Id);
            
            if ((await sameHousingQuery.ToListAsync()).Count > 0)
            {
                throw new UserFriendlyException(
                    $"{block.BlockName} - {housing.Apartment} daha önceden tanımlanmış.");
            }
        }
    }
}