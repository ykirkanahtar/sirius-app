using Abp.Authorization;
using Sirius.Authorization.Roles;
using Sirius.Authorization.Users;

namespace Sirius.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
