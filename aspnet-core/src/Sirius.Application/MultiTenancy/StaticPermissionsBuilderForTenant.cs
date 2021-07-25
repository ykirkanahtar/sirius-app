// using System.Collections.Generic;
// using System.Linq;
// using Abp.Authorization;
// using Abp.Authorization.Roles;
// using Microsoft.EntityFrameworkCore;
// using Sirius.Authorization;
// using Sirius.Authorization.Roles;
// using Sirius.EntityFrameworkCore;
//
// namespace Sirius.MultiTenancy
// {
//     public static class StaticPermissionsBuilderForTenant
//     {
//         public static void Build(SiriusDbContext context, int tenantId)
//         {
//             // Admin role
//
//             var adminRole = context.Roles.IgnoreQueryFilters()
//                 .FirstOrDefault(r => r.TenantId == tenantId && r.Name == StaticRoleNames.Tenants.Admin);
//             if (adminRole == null)
//             {
//                 adminRole = context.Roles
//                     .Add(new Role(tenantId, StaticRoleNames.Tenants.Admin, StaticRoleNames.Tenants.Admin)
//                         {IsStatic = true}).Entity;
//                 context.SaveChanges();
//             }
//
//             // Other roles
//             var siteManagementRole = context.Roles.IgnoreQueryFilters().FirstOrDefault(r =>
//                 r.TenantId == tenantId && r.Name == StaticRoleNames.Tenants.SiteManagement);
//             if (siteManagementRole == null)
//             {
//                 siteManagementRole = context.Roles
//                     .Add(new Role(tenantId, StaticRoleNames.Tenants.SiteManagement,
//                             StaticRoleNames.Tenants.SiteManagement)
//                         {IsStatic = true}).Entity;
//                 context.SaveChanges();
//             }
//
//             // var blockManagementRole = context.Roles.IgnoreQueryFilters().FirstOrDefault(r =>
//             //     r.TenantId == tenantId && r.Name == StaticRoleNames.Tenants.BlockManagement);
//             // if (blockManagementRole == null)
//             // {
//             //     blockManagementRole = context.Roles
//             //         .Add(new Role(tenantId, StaticRoleNames.Tenants.BlockManagement,
//             //                 StaticRoleNames.Tenants.BlockManagement)
//             //             {IsStatic = true}).Entity;
//             //     context.SaveChanges();
//             // }
//             //
//             // var housingOwnerRole = context.Roles.IgnoreQueryFilters().FirstOrDefault(r =>
//             //     r.TenantId == tenantId && r.Name == StaticRoleNames.Tenants.HousingOwner);
//             // if (housingOwnerRole == null)
//             // {
//             //     housingOwnerRole = context.Roles
//             //         .Add(new Role(tenantId, StaticRoleNames.Tenants.HousingOwner, StaticRoleNames.Tenants.HousingOwner)
//             //             {IsStatic = true}).Entity;
//             //     context.SaveChanges();
//             // }
//             //
//             // var housingTenantRole = context.Roles.IgnoreQueryFilters().FirstOrDefault(r =>
//             //     r.TenantId == tenantId && r.Name == StaticRoleNames.Tenants.HousingTenant);
//             // if (housingTenantRole == null)
//             // {
//             //     housingTenantRole = context.Roles
//             //         .Add(new Role(tenantId, StaticRoleNames.Tenants.HousingTenant,
//             //             StaticRoleNames.Tenants.HousingTenant) {IsStatic = true}).Entity;
//             //     context.SaveChanges();
//             // }
//
//             var customPermissions = new List<Permission>()
//             {
//                 new Permission(PermissionNames.Pages_Administration),
//                 new Permission(PermissionNames.Pages_Definitions),
//                 new Permission(PermissionNames.Pages_FinancialOperations),
//
//                 new Permission(PermissionNames.Pages_AccountBooks),
//                 new Permission(PermissionNames.Pages_CreateAccountBook),
//                 new Permission(PermissionNames.Pages_EditAccountBook),
//                 new Permission(PermissionNames.Pages_DeleteAccountBook),
//
//                 new Permission(PermissionNames.Pages_Blocks),
//                 new Permission(PermissionNames.Pages_CreateBlock),
//                 new Permission(PermissionNames.Pages_EditBlock),
//                 new Permission(PermissionNames.Pages_DeleteBlock),
//
//                 new Permission(PermissionNames.Pages_Employees),
//                 new Permission(PermissionNames.Pages_CreateEmployee),
//                 new Permission(PermissionNames.Pages_EditEmployee),
//                 new Permission(PermissionNames.Pages_DeleteEmployee),
//
//                 new Permission(PermissionNames.Pages_HousingCategories),
//                 new Permission(PermissionNames.Pages_CreateHousingCategory),
//                 new Permission(PermissionNames.Pages_EditHousingCategory),
//                 new Permission(PermissionNames.Pages_DeleteHousingCategory),
//
//                 new Permission(PermissionNames.Pages_HousingPaymentPlanGroups),
//                 new Permission(PermissionNames.Pages_CreateHousingPaymentPlanGroup),
//                 new Permission(PermissionNames.Pages_EditHousingPaymentPlanGroup),
//                 new Permission(PermissionNames.Pages_DeleteHousingPaymentPlanGroup),
//
//                 new Permission(PermissionNames.Pages_Housings),
//                 new Permission(PermissionNames.Pages_CreateHousing),
//                 new Permission(PermissionNames.Pages_EditHousing),
//                 new Permission(PermissionNames.Pages_DeleteHousing),
//                 new Permission(PermissionNames.Pages_AddPersonToHousing),
//
//                 new Permission(PermissionNames.Pages_PaymentAccounts),
//                 new Permission(PermissionNames.Pages_CreatePaymentAccount),
//                 new Permission(PermissionNames.Pages_EditPaymentAccount),
//                 new Permission(PermissionNames.Pages_DeletePaymentAccount),
//
//                 new Permission(PermissionNames.Pages_PaymentCategories),
//                 new Permission(PermissionNames.Pages_CreatePaymentCategory),
//                 new Permission(PermissionNames.Pages_EditPaymentCategory),
//                 new Permission(PermissionNames.Pages_DeletePaymentCategory),
//
//                 new Permission(PermissionNames.Pages_People),
//                 new Permission(PermissionNames.Pages_CreatePerson),
//                 new Permission(PermissionNames.Pages_EditPerson),
//                 new Permission(PermissionNames.Pages_DeletePerson),
//
//                 new Permission(PermissionNames.Pages_PeriodsForSite),
//                 new Permission(PermissionNames.Pages_PeriodsForBlock),
//                 new Permission(PermissionNames.Pages_CreatePeriodForBlock),
//                 new Permission(PermissionNames.Pages_CreatePeriodForSite),
//                 new Permission(PermissionNames.Pages_EditPeriod),
//                 new Permission(PermissionNames.Pages_DeletePeriod),
//             };
//
//
//             if (customPermissions.Any())
//             {
//                 var adminPermissionNames = context.RolePermissions
//                     .Where(p => p.RoleId == adminRole.Id && p.TenantId == tenantId && p.IsGranted).Select(p => p.Name)
//                     .ToList();
//
//                 context.Permissions.AddRange(
//                     customPermissions.Where(p => adminPermissionNames.Contains(p.Name) == false).Select(permission =>
//                         new RolePermissionSetting
//                         {
//                             TenantId = tenantId,
//                             Name = permission.Name,
//                             IsGranted = true,
//                             RoleId = adminRole.Id
//                         })
//                 );
//             }
//
//             //SiteManagement roles
//             var siteManagementPermissions = new List<Permission>()
//             {
//                 new Permission(PermissionNames.Pages_Definitions),
//                 new Permission(PermissionNames.Pages_FinancialOperations),
//
//                 new Permission(PermissionNames.Pages_AccountBooks),
//                 new Permission(PermissionNames.Pages_CreateAccountBook),
//                 new Permission(PermissionNames.Pages_EditAccountBook),
//                 new Permission(PermissionNames.Pages_DeleteAccountBook),
//
//                 new Permission(PermissionNames.Pages_Blocks),
//                 new Permission(PermissionNames.Pages_CreateBlock),
//                 new Permission(PermissionNames.Pages_EditBlock),
//                 new Permission(PermissionNames.Pages_DeleteBlock),
//
//                 new Permission(PermissionNames.Pages_Employees),
//                 new Permission(PermissionNames.Pages_CreateEmployee),
//                 new Permission(PermissionNames.Pages_EditEmployee),
//                 new Permission(PermissionNames.Pages_DeleteEmployee),
//
//                 new Permission(PermissionNames.Pages_HousingCategories),
//                 new Permission(PermissionNames.Pages_CreateHousingCategory),
//                 new Permission(PermissionNames.Pages_EditHousingCategory),
//                 new Permission(PermissionNames.Pages_DeleteHousingCategory),
//
//                 new Permission(PermissionNames.Pages_HousingPaymentPlanGroups),
//                 new Permission(PermissionNames.Pages_CreateHousingPaymentPlanGroup),
//                 new Permission(PermissionNames.Pages_EditHousingPaymentPlanGroup),
//                 new Permission(PermissionNames.Pages_DeleteHousingPaymentPlanGroup),
//
//                 new Permission(PermissionNames.Pages_Housings),
//                 new Permission(PermissionNames.Pages_CreateHousing),
//                 new Permission(PermissionNames.Pages_EditHousing),
//                 new Permission(PermissionNames.Pages_DeleteHousing),
//                 new Permission(PermissionNames.Pages_AddPersonToHousing),
//
//                 new Permission(PermissionNames.Pages_PaymentAccounts),
//                 new Permission(PermissionNames.Pages_CreatePaymentAccount),
//                 new Permission(PermissionNames.Pages_EditPaymentAccount),
//                 new Permission(PermissionNames.Pages_DeletePaymentAccount),
//
//                 new Permission(PermissionNames.Pages_PaymentCategories),
//                 new Permission(PermissionNames.Pages_CreatePaymentCategory),
//                 new Permission(PermissionNames.Pages_EditPaymentCategory),
//                 new Permission(PermissionNames.Pages_DeletePaymentCategory),
//
//                 new Permission(PermissionNames.Pages_People),
//                 new Permission(PermissionNames.Pages_CreatePerson),
//                 new Permission(PermissionNames.Pages_EditPerson),
//                 new Permission(PermissionNames.Pages_DeletePerson),
//
//                 new Permission(PermissionNames.Pages_PeriodsForSite),
//                 new Permission(PermissionNames.Pages_CreatePeriodForSite),
//                 new Permission(PermissionNames.Pages_EditPeriod),
//                 new Permission(PermissionNames.Pages_DeletePeriod),
//
//                 new Permission(PermissionNames.Pages_Users),
//             };
//
//             if (siteManagementPermissions.Any())
//             {
//                 var existingSiteManagementPermissionNames = context.RolePermissions
//                     .Where(p => p.RoleId == siteManagementRole.Id && p.TenantId == tenantId && p.IsGranted)
//                     .Select(p => p.Name)
//                     .ToList();
//
//                 context.Permissions.AddRange(
//                     siteManagementPermissions
//                         .Where(p => existingSiteManagementPermissionNames.Contains(p.Name) == false).Select(
//                             permission => new RolePermissionSetting
//                             {
//                                 TenantId = tenantId,
//                                 Name = permission.Name,
//                                 IsGranted = true,
//                                 RoleId = siteManagementRole.Id
//                             })
//                 );
//             }
//
//             //BlockManagement roles
//             // var blockManagementPermissions = new List<Permission>()
//             // {
//             //     new Permission(PermissionNames.Pages_AccountBooks),
//             //     new Permission(PermissionNames.Pages_Housings),
//             //     new Permission(PermissionNames.Pages_PaymentAccounts),
//             //     new Permission(PermissionNames.Pages_Employees),
//             //     new Permission(PermissionNames.Pages_People),
//             //     new Permission(PermissionNames.Pages_PaymentCategories),
//             //     new Permission(PermissionNames.Pages_HousingPaymentPlanGroups),
//             //     new Permission(PermissionNames.Pages_FinancialOperations),
//             //     new Permission(PermissionNames.Pages_PeriodsForBlock),
//             //     new Permission(PermissionNames.Pages_CreatePeriodForBlock),
//             // };
//             //
//             // if (blockManagementPermissions.Any())
//             // {
//             //     context.Permissions.AddRange(
//             //         blockManagementPermissions.Select(permission => new RolePermissionSetting
//             //         {
//             //             TenantId = tenantId,
//             //             Name = permission.Name,
//             //             IsGranted = true,
//             //             RoleId = blockManagementRole.Id
//             //         })
//             //     );
//             // }
//
//             //HousingOwner roles
//             
//             // var housingOwnerPermissions = new List<Permission>()
//             // {
//             // };
//             //
//             // if (housingOwnerPermissions.Any())
//             // {
//             //     context.Permissions.AddRange(
//             //         housingOwnerPermissions.Select(permission => new RolePermissionSetting
//             //         {
//             //             TenantId = tenantId,
//             //             Name = permission.Name,
//             //             IsGranted = true,
//             //             RoleId = housingOwnerRole.Id
//             //         })
//             //     );
//             // }
//             //
//             // //HousingTenant roles
//             // var housingTenantPermissions = new List<Permission>()
//             // {
//             // };
//             //
//             // if (housingTenantPermissions.Any())
//             // {
//             //     context.Permissions.AddRange(
//             //         housingTenantPermissions.Select(permission => new RolePermissionSetting
//             //         {
//             //             TenantId = tenantId,
//             //             Name = permission.Name,
//             //             IsGranted = true,
//             //             RoleId = housingTenantRole.Id
//             //         })
//             //     );
//             // }
//
//             context.SaveChanges();
//         }
//     }
// }