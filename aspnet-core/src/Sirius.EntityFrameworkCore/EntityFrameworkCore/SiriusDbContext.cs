using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using Sirius.AccountBooks;
using Sirius.Authorization.Roles;
using Sirius.Authorization.Users;
using Sirius.MultiTenancy;
using Sirius.Employees;
using Sirius.AppPaymentAccounts;
using Sirius.HousingCategories;
using Sirius.HousingPaymentPlans;
using Sirius.Housings;
using Sirius.PaymentCategories;
using Sirius.People;

namespace Sirius.EntityFrameworkCore
{
    public class SiriusDbContext : AbpZeroDbContext<Tenant, Role, User, SiriusDbContext>
    {
        /* Define a DbSet for each entity of the application */
        public virtual DbSet<AccountBook> AccountBooks { get; set; }
        public virtual DbSet<HousingPaymentPlan> HousingPaymentPlans { get; set; }
        public virtual DbSet<PaymentAccount> PaymentAccounts { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Housing> Housings { get; set; }
        public virtual DbSet<Person> People { get; set; }
        public virtual DbSet<PaymentCategory> PaymentCategories { get; set; }
        public virtual DbSet<HousingCategory> HousingCategories { get; set; }

        public SiriusDbContext(DbContextOptions<SiriusDbContext> options)
            : base(options)
        {
        }
    }
}
