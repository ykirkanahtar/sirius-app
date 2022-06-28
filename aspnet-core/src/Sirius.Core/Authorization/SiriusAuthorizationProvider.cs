using Abp.Authorization;
using Abp.Localization;
using Abp.MultiTenancy;

namespace Sirius.Authorization
{
    public class SiriusAuthorizationProvider : AuthorizationProvider
    {
        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            context.CreatePermission(PermissionNames.Pages_Users, L("Users"));
            context.CreatePermission(PermissionNames.Pages_Users_Activation, L("UsersActivation"));
            context.CreatePermission(PermissionNames.Pages_Roles, L("Roles"));
            context.CreatePermission(PermissionNames.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);
            
            context.CreatePermission(PermissionNames.Pages_AccountBooks, L("AccountBooksMenu"));
            context.CreatePermission(PermissionNames.Pages_Housings, L("HousingsMenu"));
            context.CreatePermission(PermissionNames.Pages_PaymentAccounts, L("PaymentAccountsMenu"));
            context.CreatePermission(PermissionNames.Pages_Blocks, L("BlocksMenu"));
            context.CreatePermission(PermissionNames.Pages_Employees, L("EmployeesMenu"));
            context.CreatePermission(PermissionNames.Pages_People, L("PeopleMenu"));
            context.CreatePermission(PermissionNames.Pages_PaymentCategories, L("PaymentCategoriesMenu"));
            context.CreatePermission(PermissionNames.Pages_HousingCategories, L("HousingCategoriesMenu"));
            context.CreatePermission(PermissionNames.Pages_HousingPaymentPlanGroups, L("HousingPaymentPlanGroupsMenu"));
            context.CreatePermission(PermissionNames.Pages_Administration, L("AdministrationMenu"));
            context.CreatePermission(PermissionNames.Pages_Definitions, L("DefinitionsMenu"));
            context.CreatePermission(PermissionNames.Pages_FinancialOperations, L("FinancialOperationsMenu"));
            context.CreatePermission(PermissionNames.Pages_PeriodsForSite, L("PeriodsForSiteMenu"));
            context.CreatePermission(PermissionNames.Pages_PeriodsForBlock, L("PeriodsForBlockMenu"));
            context.CreatePermission(PermissionNames.Pages_InventoryTypes, L("InventoryTypesMenu"));
            context.CreatePermission(PermissionNames.Pages_Inventories, L("InventoriesMenu"));
            context.CreatePermission(PermissionNames.Pages_Reports, L("ReportsMenu"));
            
            context.CreatePermission(PermissionNames.Pages_HousingDueReport, L("HousingDueReport"));
            
            context.CreatePermission(PermissionNames.Pages_CreateAccountBook, L("CreateAccountBook"));
            context.CreatePermission(PermissionNames.Pages_EditAccountBook, L("EditAccountBook"));
            context.CreatePermission(PermissionNames.Pages_DeleteAccountBook, L("DeleteAccountBook"));

            context.CreatePermission(PermissionNames.Pages_CreateBlock, L("CreateBlock"));
            context.CreatePermission(PermissionNames.Pages_EditBlock, L("EditBlock"));
            context.CreatePermission(PermissionNames.Pages_DeleteBlock, L("DeleteBlock"));
            
            context.CreatePermission(PermissionNames.Pages_CreateEmployee, L("CreateEmployee"));
            context.CreatePermission(PermissionNames.Pages_EditEmployee, L("EditEmployee"));
            context.CreatePermission(PermissionNames.Pages_DeleteEmployee, L("DeleteEmployee"));
            
            context.CreatePermission(PermissionNames.Pages_CreateHousingCategory, L("CreateHousingCategory"));
            context.CreatePermission(PermissionNames.Pages_EditHousingCategory, L("EditHousingCategory"));
            context.CreatePermission(PermissionNames.Pages_DeleteHousingCategory, L("DeleteHousingCategory"));
            
            context.CreatePermission(PermissionNames.Pages_CreateHousingPaymentPlanGroup, L("CreateHousingPaymentPlanGroup"));
            context.CreatePermission(PermissionNames.Pages_EditHousingPaymentPlanGroup, L("EditHousingPaymentPlanGroup"));
            context.CreatePermission(PermissionNames.Pages_DeleteHousingPaymentPlanGroup, L("DeleteHousingPaymentPlanGroup"));
            
            context.CreatePermission(PermissionNames.Pages_CreateHousing, L("CreateHousing"));
            context.CreatePermission(PermissionNames.Pages_EditHousing, L("EditHousing"));
            context.CreatePermission(PermissionNames.Pages_DeleteHousing, L("DeleteHousing"));
            context.CreatePermission(PermissionNames.Pages_AddPersonToHousing, L("AddPersonToHousing"));

            context.CreatePermission(PermissionNames.Pages_CreatePaymentAccount, L("CreatePaymentAccount"));
            context.CreatePermission(PermissionNames.Pages_EditPaymentAccount, L("EditPaymentAccount"));
            context.CreatePermission(PermissionNames.Pages_DeletePaymentAccount, L("DeletePaymentAccount"));
            
            context.CreatePermission(PermissionNames.Pages_CreatePaymentCategory, L("CreatePaymentCategory"));
            context.CreatePermission(PermissionNames.Pages_EditPaymentCategory, L("EditPaymentCategory"));
            context.CreatePermission(PermissionNames.Pages_DeletePaymentCategory, L("DeletePaymentCategory"));
            
            context.CreatePermission(PermissionNames.Pages_CreatePerson, L("CreatePerson"));
            context.CreatePermission(PermissionNames.Pages_EditPerson, L("EditPerson"));
            context.CreatePermission(PermissionNames.Pages_DeletePerson, L("DeletePerson"));
            
            context.CreatePermission(PermissionNames.Pages_CreatePeriodForBlock, L("CreatePeriodForBlock"));
            context.CreatePermission(PermissionNames.Pages_CreatePeriodForSite, L("CreatePeriodForSite"));
            context.CreatePermission(PermissionNames.Pages_EditPeriod, L("EditPeriod"));
            context.CreatePermission(PermissionNames.Pages_DeletePeriod, L("DeletePeriod"));
            
            context.CreatePermission(PermissionNames.Pages_CreateInventoryType, L("CreateInventoryType"));
            context.CreatePermission(PermissionNames.Pages_EditInventoryType, L("EditInventoryType"));
            context.CreatePermission(PermissionNames.Pages_DeleteInventoryType, L("DeleteInventoryType"));
            
            context.CreatePermission(PermissionNames.Pages_CreateInventory, L("CreateInventory"));
            context.CreatePermission(PermissionNames.Pages_EditInventory, L("EditInventory"));
            context.CreatePermission(PermissionNames.Pages_DeleteInventory, L("DeleteInventory"));
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, SiriusConsts.LocalizationSourceName);
        }
    }
}
