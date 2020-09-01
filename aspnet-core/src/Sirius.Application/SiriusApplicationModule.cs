using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Sirius.Authorization;

namespace Sirius
{
    [DependsOn(
        typeof(SiriusCoreModule), 
        typeof(AbpAutoMapperModule))]
    public class SiriusApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<SiriusAuthorizationProvider>();
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(SiriusApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );
        }
    }
}
