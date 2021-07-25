using System.Collections.Generic;
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

            if (_tenantId != 1)
            {
                CreateCustomRoleAndPermissions();
            }
        }

        private void CreateRolesAndUsers()
        {
            // Admin role

            var adminRole = _context.Roles.IgnoreQueryFilters()
                .FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.Admin);
            if (adminRole == null)
            {
                adminRole = _context.Roles
                    .Add(new Role(_tenantId, StaticRoleNames.Tenants.Admin, StaticRoleNames.Tenants.Admin)
                        {IsStatic = true}).Entity;
                _context.SaveChanges();
            }

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
                _context.SaveChanges();
            }

            // Admin user

            var adminUser = _context.Users.IgnoreQueryFilters()
                .FirstOrDefault(u => u.TenantId == _tenantId && u.UserName == AbpUserBase.AdminUserName);
            if (adminUser == null)
            {
                adminUser = User.CreateTenantAdminUser(_tenantId, "admin@defaulttenant.com");
                adminUser.Password =
                    new PasswordHasher<User>(new OptionsWrapper<PasswordHasherOptions>(new PasswordHasherOptions()))
                        .HashPassword(adminUser, "123qwe");
                adminUser.IsEmailConfirmed = true;
                adminUser.IsActive = true;

                _context.Users.Add(adminUser);
                _context.SaveChanges();

                // Assign Admin role to admin user
                _context.UserRoles.Add(new UserRole(_tenantId, adminUser.Id, adminRole.Id));
                _context.SaveChanges();
            }
        }

        private void CreateCustomRoleAndPermissions()
        {
            var siteManagementRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r =>
                r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.SiteManagement);
            if (siteManagementRole == null)
            {
                siteManagementRole = _context.Roles
                    .Add(new Role(_tenantId, StaticRoleNames.Tenants.SiteManagement,
                            StaticRoleNames.Tenants.SiteManagement)
                        {IsStatic = true}).Entity;
                _context.SaveChanges();
            }
            
            if (GetSiteManagementPermissions().Any())
            {
                var grantedPermissions = _context.Permissions.IgnoreQueryFilters()
                    .OfType<RolePermissionSetting>()
                    .Where(p => p.TenantId == _tenantId && p.RoleId == siteManagementRole.Id)
                    .Select(p => p.Name)
                    .ToList();
            
                var permissions = GetSiteManagementPermissions()
                    .Where(p => !grantedPermissions.Contains(p.Name))
                    .ToList();
            
                if (permissions.Any())
                {
                    _context.Permissions.AddRange(
                        permissions.Select(permission => new RolePermissionSetting
                        {
                            TenantId = _tenantId,
                            Name = permission.Name,
                            IsGranted = true,
                            RoleId = siteManagementRole.Id
                        })
                    );
                    _context.SaveChanges();
                }
            }
            
            var siteAuditorRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r =>
                r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.SiteAuditor);
            if (siteAuditorRole == null)
            {
                siteAuditorRole = _context.Roles
                    .Add(new Role(_tenantId, StaticRoleNames.Tenants.SiteAuditor,
                            StaticRoleNames.Tenants.SiteAuditor)
                        {IsStatic = true}).Entity;
                _context.SaveChanges();
            }

            if (GetSiteAuditorPermissions().Any())
            {
                var grantedPermissions = _context.Permissions.IgnoreQueryFilters()
                    .OfType<RolePermissionSetting>()
                    .Where(p => p.TenantId == _tenantId && p.RoleId == siteAuditorRole.Id)
                    .Select(p => p.Name)
                    .ToList();

                var permissions = GetSiteAuditorPermissions()
                    .Where(p => !grantedPermissions.Contains(p.Name))
                    .ToList();

                if (permissions.Any())
                {
                    _context.Permissions.AddRange(
                        permissions.Select(permission => new RolePermissionSetting
                        {
                            TenantId = _tenantId,
                            Name = permission.Name,
                            IsGranted = true,
                            RoleId = siteAuditorRole.Id
                        })
                    );
                    _context.SaveChanges();
                }
            }
        }

        private static List<Permission> GetSiteManagementPermissions()
        {
            return new List<Permission>()
            {
                new Permission(PermissionNames.Pages_Definitions),
                new Permission(PermissionNames.Pages_FinancialOperations),

                new Permission(PermissionNames.Pages_AccountBooks),
                new Permission(PermissionNames.Pages_CreateAccountBook),
                new Permission(PermissionNames.Pages_EditAccountBook),
                new Permission(PermissionNames.Pages_DeleteAccountBook),

                new Permission(PermissionNames.Pages_Blocks),
                new Permission(PermissionNames.Pages_CreateBlock),
                new Permission(PermissionNames.Pages_EditBlock),
                new Permission(PermissionNames.Pages_DeleteBlock),

                new Permission(PermissionNames.Pages_Employees),
                new Permission(PermissionNames.Pages_CreateEmployee),
                new Permission(PermissionNames.Pages_EditEmployee),
                new Permission(PermissionNames.Pages_DeleteEmployee),

                new Permission(PermissionNames.Pages_HousingCategories),
                new Permission(PermissionNames.Pages_CreateHousingCategory),
                new Permission(PermissionNames.Pages_EditHousingCategory),
                new Permission(PermissionNames.Pages_DeleteHousingCategory),

                new Permission(PermissionNames.Pages_HousingPaymentPlanGroups),
                new Permission(PermissionNames.Pages_CreateHousingPaymentPlanGroup),
                new Permission(PermissionNames.Pages_EditHousingPaymentPlanGroup),
                new Permission(PermissionNames.Pages_DeleteHousingPaymentPlanGroup),

                new Permission(PermissionNames.Pages_Housings),
                new Permission(PermissionNames.Pages_CreateHousing),
                new Permission(PermissionNames.Pages_EditHousing),
                new Permission(PermissionNames.Pages_DeleteHousing),
                new Permission(PermissionNames.Pages_AddPersonToHousing),

                new Permission(PermissionNames.Pages_PaymentAccounts),
                new Permission(PermissionNames.Pages_CreatePaymentAccount),
                new Permission(PermissionNames.Pages_EditPaymentAccount),
                new Permission(PermissionNames.Pages_DeletePaymentAccount),

                new Permission(PermissionNames.Pages_PaymentCategories),
                new Permission(PermissionNames.Pages_CreatePaymentCategory),
                new Permission(PermissionNames.Pages_EditPaymentCategory),
                new Permission(PermissionNames.Pages_DeletePaymentCategory),

                new Permission(PermissionNames.Pages_People),
                new Permission(PermissionNames.Pages_CreatePerson),
                new Permission(PermissionNames.Pages_EditPerson),
                new Permission(PermissionNames.Pages_DeletePerson),

                new Permission(PermissionNames.Pages_PeriodsForSite),
                new Permission(PermissionNames.Pages_CreatePeriodForSite),
                new Permission(PermissionNames.Pages_EditPeriod),
                new Permission(PermissionNames.Pages_DeletePeriod),
                
                new Permission(PermissionNames.Pages_Inventories),
                new Permission(PermissionNames.Pages_CreateInventory),
                new Permission(PermissionNames.Pages_EditInventory),
                new Permission(PermissionNames.Pages_DeleteInventory),
                
                new Permission(PermissionNames.Pages_InventoryTypes),
                new Permission(PermissionNames.Pages_CreateInventoryType),
                new Permission(PermissionNames.Pages_EditInventoryType),
                new Permission(PermissionNames.Pages_DeleteInventoryType),

                new Permission(PermissionNames.Pages_Users),
            };
        }

        private static List<Permission> GetSiteAuditorPermissions()
        {
            return new List<Permission>()
            {
                new Permission(PermissionNames.Pages_Definitions),
                new Permission(PermissionNames.Pages_FinancialOperations),

                new Permission(PermissionNames.Pages_AccountBooks),
                
                new Permission(PermissionNames.Pages_Blocks),
                
                new Permission(PermissionNames.Pages_Employees),

                new Permission(PermissionNames.Pages_HousingCategories),

                new Permission(PermissionNames.Pages_HousingPaymentPlanGroups),

                new Permission(PermissionNames.Pages_Housings),

                new Permission(PermissionNames.Pages_PaymentAccounts),

                new Permission(PermissionNames.Pages_PaymentCategories),

                new Permission(PermissionNames.Pages_People),

                new Permission(PermissionNames.Pages_PeriodsForSite),
                
                new Permission(PermissionNames.Pages_Inventories),

                new Permission(PermissionNames.Pages_InventoryTypes),

                new Permission(PermissionNames.Pages_Users),
            };
        }


        private static List<Permission> GetAllPermissions()
        {
            return new List<Permission>()
            {
                new Permission(PermissionNames.Pages_Administration),
                new Permission(PermissionNames.Pages_Definitions),
                new Permission(PermissionNames.Pages_FinancialOperations),

                new Permission(PermissionNames.Pages_AccountBooks),
                new Permission(PermissionNames.Pages_CreateAccountBook),
                new Permission(PermissionNames.Pages_EditAccountBook),
                new Permission(PermissionNames.Pages_DeleteAccountBook),

                new Permission(PermissionNames.Pages_Blocks),
                new Permission(PermissionNames.Pages_CreateBlock),
                new Permission(PermissionNames.Pages_EditBlock),
                new Permission(PermissionNames.Pages_DeleteBlock),

                new Permission(PermissionNames.Pages_Employees),
                new Permission(PermissionNames.Pages_CreateEmployee),
                new Permission(PermissionNames.Pages_EditEmployee),
                new Permission(PermissionNames.Pages_DeleteEmployee),

                new Permission(PermissionNames.Pages_HousingCategories),
                new Permission(PermissionNames.Pages_CreateHousingCategory),
                new Permission(PermissionNames.Pages_EditHousingCategory),
                new Permission(PermissionNames.Pages_DeleteHousingCategory),

                new Permission(PermissionNames.Pages_HousingPaymentPlanGroups),
                new Permission(PermissionNames.Pages_CreateHousingPaymentPlanGroup),
                new Permission(PermissionNames.Pages_EditHousingPaymentPlanGroup),
                new Permission(PermissionNames.Pages_DeleteHousingPaymentPlanGroup),

                new Permission(PermissionNames.Pages_Housings),
                new Permission(PermissionNames.Pages_CreateHousing),
                new Permission(PermissionNames.Pages_EditHousing),
                new Permission(PermissionNames.Pages_DeleteHousing),
                new Permission(PermissionNames.Pages_AddPersonToHousing),

                new Permission(PermissionNames.Pages_PaymentAccounts),
                new Permission(PermissionNames.Pages_CreatePaymentAccount),
                new Permission(PermissionNames.Pages_EditPaymentAccount),
                new Permission(PermissionNames.Pages_DeletePaymentAccount),

                new Permission(PermissionNames.Pages_PaymentCategories),
                new Permission(PermissionNames.Pages_CreatePaymentCategory),
                new Permission(PermissionNames.Pages_EditPaymentCategory),
                new Permission(PermissionNames.Pages_DeletePaymentCategory),

                new Permission(PermissionNames.Pages_People),
                new Permission(PermissionNames.Pages_CreatePerson),
                new Permission(PermissionNames.Pages_EditPerson),
                new Permission(PermissionNames.Pages_DeletePerson),

                new Permission(PermissionNames.Pages_PeriodsForSite),
                new Permission(PermissionNames.Pages_PeriodsForBlock),
                new Permission(PermissionNames.Pages_CreatePeriodForBlock),
                new Permission(PermissionNames.Pages_CreatePeriodForSite),
                new Permission(PermissionNames.Pages_EditPeriod),
                new Permission(PermissionNames.Pages_DeletePeriod),
                
                new Permission(PermissionNames.Pages_Inventories),
                new Permission(PermissionNames.Pages_CreateInventory),
                new Permission(PermissionNames.Pages_EditInventory),
                new Permission(PermissionNames.Pages_DeleteInventory),
                
                new Permission(PermissionNames.Pages_InventoryTypes),
                new Permission(PermissionNames.Pages_CreateInventoryType),
                new Permission(PermissionNames.Pages_EditInventoryType),
                new Permission(PermissionNames.Pages_DeleteInventoryType),
            };
        }
    }
}