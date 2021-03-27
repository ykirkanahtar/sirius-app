using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Sirius.Configuration;
using Sirius.Web;

namespace Sirius.EntityFrameworkCore
{
    /* This class is needed to run "dotnet ef ..." commands from command line on development. Not used anywhere else */
    public class SiriusDbContextFactory : IDesignTimeDbContextFactory<SiriusDbContext>
    {
        public SiriusDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<SiriusDbContext>();
            var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder());

            SiriusDbContextConfigurer.Configure(builder, configuration.GetConnectionString(SiriusConsts.ConnectionStringName));

            return new SiriusDbContext(builder.Options);
        }
    }
}
