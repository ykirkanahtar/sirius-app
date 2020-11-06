using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using Sirius.Authorization;
using Sirius.Authorization.Roles;
using Sirius.Authorization.Users;
using System.Collections.Generic;

namespace Sirius.EntityFrameworkCore.Seed.Tenants
{
    public class TenantRoleAndUserBuilder
    {
        private readonly SiriusDbContext _context;
        private readonly int _tenantId;

        public TenantRoleAndUserBuilder(SiriusDbContext context, int tenantId)
        {
            _context = context;
            _tenantId = tenantId;
        }

        public void Create()
        {
            CreateRolesAndUsers();
        }

        private void CreateRolesAndUsers()
        {
            // Admin role

            var adminRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.Admin);
            if (adminRole == null)
            {
                adminRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.Admin, StaticRoleNames.Tenants.Admin) { IsStatic = true }).Entity;
                _context.SaveChanges();
            }

            // Other roles

            var siteManagerRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.SiteManager);
            if (siteManagerRole == null)
            {
                siteManagerRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.SiteManager, StaticRoleNames.Tenants.SiteManager) { IsStatic = true }).Entity;
                _context.SaveChanges();
            }

            var blockManagerRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.BlockManager);
            if (blockManagerRole == null)
            {
                blockManagerRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.BlockManager, StaticRoleNames.Tenants.BlockManager) { IsStatic = true }).Entity;
                _context.SaveChanges();
            }

            var housingOwnerRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.HousingOwner);
            if (housingOwnerRole == null)
            {
                housingOwnerRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.HousingOwner, StaticRoleNames.Tenants.HousingOwner) { IsStatic = true }).Entity;
                _context.SaveChanges();
            }

            var housingTenantRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.HousingTenant);
            if (housingTenantRole == null)
            {
                housingTenantRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.HousingTenant, StaticRoleNames.Tenants.HousingTenant) { IsStatic = true }).Entity;
                _context.SaveChanges();
            }

            var customPermissions = new List<Permission>()
            {
                new Permission(PermissionNames.Pages_AccountBooks),
                new Permission(PermissionNames.Pages_Housings),
                new Permission(PermissionNames.Pages_PaymentAccounts),
                new Permission(PermissionNames.Pages_Blocks),
                new Permission(PermissionNames.Pages_Employees),
                new Permission(PermissionNames.Pages_People),
                new Permission(PermissionNames.Pages_PaymentCategories),
                new Permission(PermissionNames.Pages_HousingCategories),
                new Permission(PermissionNames.Pages_HousingPaymentPlans),
            };

            // Grant all permissions to admin role

            var grantedPermissions = _context.Permissions.IgnoreQueryFilters()
                .OfType<RolePermissionSetting>()
                .Where(p => p.TenantId == _tenantId && p.RoleId == adminRole.Id)
                .Select(p => p.Name)
                .ToList();

            var permissions = PermissionFinder
                .GetAllPermissions(new SiriusAuthorizationProvider())
                .Where(p => p.MultiTenancySides.HasFlag(MultiTenancySides.Tenant) &&
                            !grantedPermissions.Contains(p.Name))
                .ToList();

            if (customPermissions.Any())
            {
                _context.Permissions.AddRange(
                    customPermissions.Select(permission => new RolePermissionSetting
                    {
                        TenantId = _tenantId,
                        Name = permission.Name,
                        IsGranted = true,
                        RoleId = adminRole.Id
                    })
                );
            }

            if (permissions.Any())
            {
                _context.Permissions.AddRange(
                    permissions.Select(permission => new RolePermissionSetting
                    {
                        TenantId = _tenantId,
                        Name = permission.Name,
                        IsGranted = true,
                        RoleId = adminRole.Id
                    })
                );
            }

            //SiteManager roles
            var siteManagerPermissions = new List<Permission>()
            {
                new Permission(PermissionNames.Pages_AccountBooks),
                new Permission(PermissionNames.Pages_Housings),
                new Permission(PermissionNames.Pages_PaymentAccounts),
                new Permission(PermissionNames.Pages_Blocks),
                new Permission(PermissionNames.Pages_Employees),
                new Permission(PermissionNames.Pages_People),
                new Permission(PermissionNames.Pages_PaymentCategories),
                new Permission(PermissionNames.Pages_HousingCategories),
                new Permission(PermissionNames.Pages_HousingPaymentPlans),
            };

            if (siteManagerPermissions.Any())
            {
                _context.Permissions.AddRange(
                    siteManagerPermissions.Select(permission => new RolePermissionSetting
                    {
                        TenantId = _tenantId,
                        Name = permission.Name,
                        IsGranted = true,
                        RoleId = siteManagerRole.Id
                    })
                );
            }

            //BlockManager roles
            var blockManagerPermissions = new List<Permission>()
            {
                new Permission(PermissionNames.Pages_AccountBooks),
                new Permission(PermissionNames.Pages_Housings),
                new Permission(PermissionNames.Pages_PaymentAccounts),
                new Permission(PermissionNames.Pages_Employees),
                new Permission(PermissionNames.Pages_People),
                new Permission(PermissionNames.Pages_PaymentCategories),
                new Permission(PermissionNames.Pages_HousingPaymentPlans),
            };

            if (blockManagerPermissions.Any())
            {
                _context.Permissions.AddRange(
                    blockManagerPermissions.Select(permission => new RolePermissionSetting
                    {
                        TenantId = _tenantId,
                        Name = permission.Name,
                        IsGranted = true,
                        RoleId = blockManagerRole.Id
                    })
                );
            }

            //HousingOwner roles
            var housingOwnerPermissions = new List<Permission>()
            {

            };

            if (housingOwnerPermissions.Any())
            {
                _context.Permissions.AddRange(
                    housingOwnerPermissions.Select(permission => new RolePermissionSetting
                    {
                        TenantId = _tenantId,
                        Name = permission.Name,
                        IsGranted = true,
                        RoleId = housingOwnerRole.Id
                    })
                );
            }

            //HousingTenant roles
            var housingTenantPermissions = new List<Permission>()
            {


            };

            if (housingTenantPermissions.Any())
            {
                _context.Permissions.AddRange(
                    housingTenantPermissions.Select(permission => new RolePermissionSetting
                    {
                        TenantId = _tenantId,
                        Name = permission.Name,
                        IsGranted = true,
                        RoleId = housingTenantRole.Id
                    })
                );
            }

            _context.SaveChanges();

            // Admin user

            var adminUser = _context.Users.IgnoreQueryFilters().FirstOrDefault(u => u.TenantId == _tenantId && u.UserName == AbpUserBase.AdminUserName);
            if (adminUser == null)
            {
                adminUser = User.CreateTenantAdminUser(_tenantId, "admin@defaulttenant.com");
                adminUser.Password = new PasswordHasher<User>(new OptionsWrapper<PasswordHasherOptions>(new PasswordHasherOptions())).HashPassword(adminUser, "123qwe");
                adminUser.IsEmailConfirmed = true;
                adminUser.IsActive = true;

                _context.Users.Add(adminUser);
                _context.SaveChanges();

                // Assign Admin role to admin user
                _context.UserRoles.Add(new UserRole(_tenantId, adminUser.Id, adminRole.Id));
                _context.SaveChanges();
            }
        }
    }
}
