using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace Sirius.EntityFrameworkCore
{
    public static class SiriusDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<SiriusDbContext> builder, string connectionString)
        {
            builder.UseSqlServer(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<SiriusDbContext> builder, DbConnection connection)
        {
            builder.UseSqlServer(connection);
        }
    }
}
