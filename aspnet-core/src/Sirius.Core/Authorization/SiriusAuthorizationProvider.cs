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
            context.CreatePermission(PermissionNames.Pages_Roles, L("Roles"));
            context.CreatePermission(PermissionNames.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);
            
            context.CreatePermission(PermissionNames.Pages_AccountBooks, L("AccountBooks"));
            context.CreatePermission(PermissionNames.Pages_Housings, L("Housings"));
            context.CreatePermission(PermissionNames.Pages_PaymentAccounts, L("PaymentAccounts"));
            context.CreatePermission(PermissionNames.Pages_Blocks, L("Blocks"));
            context.CreatePermission(PermissionNames.Pages_Employees, L("Employees"));
            context.CreatePermission(PermissionNames.Pages_People, L("People"));
            context.CreatePermission(PermissionNames.Pages_PaymentCategories, L("PaymentCategories"));
            context.CreatePermission(PermissionNames.Pages_HousingCategories, L("HousingCategories"));
            context.CreatePermission(PermissionNames.Pages_HousingPaymentPlans, L("HousingPaymentPlans"));
            context.CreatePermission(PermissionNames.Pages_Administration, L("Administration"));
            context.CreatePermission(PermissionNames.Pages_Definitions, L("Definitions"));
            context.CreatePermission(PermissionNames.Pages_FinancialOperations, L("FinancialOperations"));
            
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
            
            context.CreatePermission(PermissionNames.Pages_CreateHousingPaymentPlan, L("CreateHousingPaymentPlan"));
            context.CreatePermission(PermissionNames.Pages_EditHousingPaymentPlan, L("EditHousingPaymentPlan"));
            context.CreatePermission(PermissionNames.Pages_DeleteHousingPaymentPlan, L("DeleteHousingPaymentPlan"));
            
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
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, SiriusConsts.LocalizationSourceName);
        }
    }
}
