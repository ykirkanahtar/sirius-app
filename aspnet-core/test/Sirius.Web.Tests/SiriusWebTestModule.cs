using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Sirius.EntityFrameworkCore;
using Sirius.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Sirius.Web.Tests
{
    [DependsOn(
        typeof(SiriusWebMvcModule),
        typeof(AbpAspNetCoreTestBaseModule)
    )]
    public class SiriusWebTestModule : AbpModule
    {
        public SiriusWebTestModule(SiriusEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
        } 
        
        public override void PreInitialize()
        {
            Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(SiriusWebTestModule).GetAssembly());
        }
        
        public override void PostInitialize()
        {
            IocManager.Resolve<ApplicationPartManager>()
                .AddApplicationPartsIfNotAddedBefore(typeof(SiriusWebMvcModule).Assembly);
        }
    }
}