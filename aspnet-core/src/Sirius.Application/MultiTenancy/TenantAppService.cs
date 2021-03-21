﻿using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore.Uow;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Abp.MultiTenancy;
using Abp.Runtime.Security;
using Abp.Runtime.Session;
using Sirius.Authorization;
using Sirius.Authorization.Roles;
using Sirius.Authorization.Users;
using Sirius.Editions;
using Sirius.MultiTenancy.Dto;
using Microsoft.AspNetCore.Identity;
using Sirius.EntityFrameworkCore;
using Sirius.EntityFrameworkCore.Seed.Tenants;
using Sirius.PaymentCategories;
using Sirius.Shared.Constants;
using Sirius.Shared.Enums;

namespace Sirius.MultiTenancy
{
    [AbpAuthorize(PermissionNames.Pages_Tenants)]
    public class TenantAppService :
        AsyncCrudAppService<Tenant, TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>,
        ITenantAppService
    {
        private readonly TenantManager _tenantManager;
        private readonly EditionManager _editionManager;
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly IAbpZeroDbMigrator _abpZeroDbMigrator;
        private readonly IPaymentCategoryManager _paymentCategoryManager;

        public TenantAppService(
            IRepository<Tenant, int> repository,
            TenantManager tenantManager,
            EditionManager editionManager,
            UserManager userManager,
            RoleManager roleManager,
            IAbpZeroDbMigrator abpZeroDbMigrator,
            IPaymentCategoryManager paymentCategoryManager)
            : base(repository)
        {
            _tenantManager = tenantManager;
            _editionManager = editionManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _abpZeroDbMigrator = abpZeroDbMigrator;
            _paymentCategoryManager = paymentCategoryManager;
        }

        public override async Task<TenantDto> CreateAsync(CreateTenantDto input)
        {
            CheckCreatePermission();

            // Create tenant
            var tenant = ObjectMapper.Map<Tenant>(input);
            tenant.ConnectionString = input.ConnectionString.IsNullOrEmpty()
                ? null
                : SimpleStringCipher.Instance.Encrypt(input.ConnectionString);

            var defaultEdition = await _editionManager.FindByNameAsync(EditionManager.DefaultEditionName);
            if (defaultEdition != null)
            {
                tenant.EditionId = defaultEdition.Id;
            }

            await _tenantManager.CreateAsync(tenant);

            await CurrentUnitOfWork.SaveChangesAsync(); // To get new tenant's id.

            // Create tenant database
            _abpZeroDbMigrator.CreateOrMigrateForTenant(tenant);

            // We are working entities of new tenant, so changing tenant filter
            using (CurrentUnitOfWork.SetTenantId(tenant.Id))
            {
                // Create static roles for new tenant
                CheckErrors(await _roleManager.CreateStaticRoles(tenant.Id));

                await CurrentUnitOfWork.SaveChangesAsync(); // To get static role ids

                // Grant all permissions to admin role
                var adminRole = _roleManager.Roles.Single(r => r.Name == StaticRoleNames.Tenants.Admin);
                await _roleManager.GrantAllPermissionsAsync(adminRole);

                // Create admin user for the tenant
                var adminUser = User.CreateTenantAdminUser(tenant.Id, input.AdminEmailAddress);
                await _userManager.InitializeOptionsAsync(tenant.Id);
                CheckErrors(await _userManager.CreateAsync(adminUser, User.DefaultPassword));
                await CurrentUnitOfWork.SaveChangesAsync(); // To get admin user's id

                // Assign admin user to role!
                CheckErrors(await _userManager.AddToRoleAsync(adminUser, adminRole.Name));
                await CurrentUnitOfWork.SaveChangesAsync();

                StaticPermissionsBuilderForTenant.Build(CurrentUnitOfWork.GetDbContext<SiriusDbContext>(), tenant.Id);

                //Custom changes
                // var housingDuePaymentCategory = PaymentCategory.Create(
                //     id: SequentialGuidGenerator.Instance.Create(),
                //     tenantId: tenant.Id,
                //     paymentCategoryName: HousingDueType.RegularHousingDue.ToString(),
                //     housingDueType: HousingDueType.RegularHousingDue,
                //     isValidForAllPeriods: true,
                //     defaultFromPaymentAccountId: null,
                //     defaultToPaymentAccountId: null,
                //     showInLists: false);
                // await _paymentCategoryManager.CreateAsync(housingDuePaymentCategory);

                // var transferForHousingDuePaymentCategory = PaymentCategory.Create(
                //     id: SequentialGuidGenerator.Instance.Create(),
                //     tenantId: tenant.Id,
                //     paymentCategoryName: HousingDueType.TransferForRegularHousingDue.ToString(),
                //     housingDueType: HousingDueType.TransferForRegularHousingDue,
                //     isValidForAllPeriods: true,
                //     defaultFromPaymentAccountId: null,
                //     defaultToPaymentAccountId: null,
                //     showInLists: false);
                // await _paymentCategoryManager.CreateAsync(transferForHousingDuePaymentCategory);
                //
                // var nettingPaymentCategory = PaymentCategory.Create(
                //     id: SequentialGuidGenerator.Instance.Create(),
                //     tenantId: tenant.Id,
                //     paymentCategoryName: HousingDueType.Netting.ToString(),
                //     housingDueType: HousingDueType.Netting,
                //     isValidForAllPeriods: false,
                //     defaultFromPaymentAccountId: null,
                //     defaultToPaymentAccountId: null,
                //     showInLists: false);
                // await _paymentCategoryManager.CreateAsync(nettingPaymentCategory);
                //
                // var transferForPaymentAccountCategory = PaymentCategory.Create(
                //     id: SequentialGuidGenerator.Instance.Create(),
                //     tenantId: tenant.Id, 
                //     paymentCategoryName: AppConstants.TransferForPaymentAccount,
                //     housingDueType: null, 
                //     isValidForAllPeriods: true,
                //     defaultFromPaymentAccountId: null,
                //     defaultToPaymentAccountId: null,
                //     showInLists: false, 
                //     editInAccountBook: false);
                // await _paymentCategoryManager.CreateAsync(transferForPaymentAccountCategory);
            }

            return MapToEntityDto(tenant);
        }

        protected override IQueryable<Tenant> CreateFilteredQuery(PagedTenantResultRequestDto input)
        {
            return Repository.GetAll()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(),
                    x => x.TenancyName.Contains(input.Keyword) || x.Name.Contains(input.Keyword))
                .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);
        }

        protected override void MapToEntity(TenantDto updateInput, Tenant entity)
        {
            // Manually mapped since TenantDto contains non-editable properties too.
            entity.Name = updateInput.Name;
            entity.TenancyName = updateInput.TenancyName;
            entity.IsActive = updateInput.IsActive;
        }

        public override async Task DeleteAsync(EntityDto<int> input)
        {
            CheckDeletePermission();

            var tenant = await _tenantManager.GetByIdAsync(input.Id);
            await _tenantManager.DeleteAsync(tenant);
        }

        private void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}