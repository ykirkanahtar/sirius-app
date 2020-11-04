using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sirius.Housings;

namespace Sirius.EntityFrameworkCore.Repositories
{
    public interface IHousingRepository : IRepository<Housing, Guid>
    {
        
    }
    
    public class HousingRepository : SiriusRepositoryBase<Housing, Guid>, IHousingRepository
    {
        public HousingRepository(IDbContextProvider<SiriusDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public override async Task<Housing> GetAsync(Guid id)
        {
            var query = this.GetQueryable().Where(p => p.Id == id).Include(p => p.Block)
                .Include(p => p.HousingCategory);
            var entity = await query.FirstOrDefaultAsync();
            return (object) entity != null ? entity : throw new EntityNotFoundException(typeof(Housing), id);
        }
    }
}