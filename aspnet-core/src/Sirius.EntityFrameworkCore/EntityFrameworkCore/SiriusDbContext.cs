using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using Sirius.Authorization.Roles;
using Sirius.Authorization.Users;
using Sirius.MultiTenancy;

namespace Sirius.EntityFrameworkCore
{
    public class SiriusDbContext : AbpZeroDbContext<Tenant, Role, User, SiriusDbContext>
    {
        /* Define a DbSet for each entity of the application */
        
        public SiriusDbContext(DbContextOptions<SiriusDbContext> options)
            : base(options)
        {
        }
    }
}
