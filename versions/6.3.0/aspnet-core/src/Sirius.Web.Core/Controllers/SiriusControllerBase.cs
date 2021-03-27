using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace Sirius.Controllers
{
    public abstract class SiriusControllerBase: AbpController
    {
        protected SiriusControllerBase()
        {
            LocalizationSourceName = SiriusConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
