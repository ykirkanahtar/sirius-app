import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {HttpClientJsonpModule} from '@angular/common/http';
import {HttpClientModule} from '@angular/common/http';
import {ModalModule} from 'ngx-bootstrap/modal';
import {BsDropdownModule} from 'ngx-bootstrap/dropdown';
import {CollapseModule} from 'ngx-bootstrap/collapse';
import {TabsModule} from 'ngx-bootstrap/tabs';
import {NgxPaginationModule} from 'ngx-pagination';
import {AppRoutingModule} from './app-routing.module';
import {AppComponent} from './app.component';
import {ServiceProxyModule} from '@shared/service-proxies/service-proxy.module';
import {SharedModule} from '@shared/shared.module';
import {HomeComponent} from '@app/home/home.component';
import {AboutComponent} from '@app/about/about.component';
// tenants
import {TenantsComponent} from '@app/tenants/tenants.component';
import {CreateTenantDialogComponent} from './tenants/create-tenant/create-tenant-dialog.component';
import {EditTenantDialogComponent} from './tenants/edit-tenant/edit-tenant-dialog.component';
// roles
import {RolesComponent} from '@app/roles/roles.component';
import {CreateRoleDialogComponent} from './roles/create-role/create-role-dialog.component';
import {EditRoleDialogComponent} from './roles/edit-role/edit-role-dialog.component';
// users
import {UsersComponent} from '@app/users/users.component';
import {CreateUserDialogComponent} from '@app/users/create-user/create-user-dialog.component';
import {EditUserDialogComponent} from '@app/users/edit-user/edit-user-dialog.component';
import {ChangePasswordComponent} from './users/change-password/change-password.component';
import {ResetPasswordDialogComponent} from './users/reset-password/reset-password.component';
// layout
import {HeaderComponent} from './layout/header.component';
import {HeaderLeftNavbarComponent} from './layout/header-left-navbar.component';
import {HeaderLanguageMenuComponent} from './layout/header-language-menu.component';
import {HeaderUserMenuComponent} from './layout/header-user-menu.component';
import {FooterComponent} from './layout/footer.component';
import {SidebarComponent} from './layout/sidebar.component';
import {SidebarLogoComponent} from './layout/sidebar-logo.component';
import {SidebarUserPanelComponent} from './layout/sidebar-user-panel.component';
import {SidebarMenuComponent} from './layout/sidebar-menu.component';

// custom-components
import {CreateHousingDialogComponent} from './housings/create-housing/create-housing-dialog.component';
import {EditHousingDialogComponent} from './housings/edit-housing/edit-housing-dialog.component';
import {AddPersonDialogComponent} from './housings/add-or-edit-person/add-person-dialog.component';
import {AccountActivitiesDialogComponent} from './housings/account-activities/account-activities.component';
import {HousingPeopleDialogComponent} from './housings/housing-people/housing-people.component';
import {BlocksComponent} from './blocks/blocks.component';
import {CreateBlockDialogComponent} from './blocks/create-block/create-block-dialog.component';
import {EditBlockDialogComponent} from './blocks/edit-block/edit-block-dialog.component';
import {EmployeesComponent} from './employees/employees.component';
import {CreateEmployeeDialogComponent} from './employees/create-employee/create-employee-dialog.component';
import {EditEmployeeDialogComponent} from './employees/edit-employee/edit-employee-dialog.component';
import {PeopleComponent} from './people/people.component';
import {CreatePersonDialogComponent} from './people/create-person/create-person-dialog.component';
import {EditPersonDialogComponent} from './people/edit-person/edit-person-dialog.component';
import {AccountBooksComponent} from './account-books/account-books.component';
import {PaymentAccountsComponent} from './payment-accounts/payment-accounts.component';
import {CreatePaymentAccountDialogComponent} from './payment-accounts/create-payment-account/create-payment-account-dialog.component';
import {CreateAccountBookDialogComponent} from './account-books/create-account-book/create-account-book-dialog.component';
import {EditAccountBookDialogComponent} from './account-books/edit-account-book/edit-account-book-dialog.component';
import {HousingsComponent} from './housings/housings.component';
import {EditPaymentAccountDialogComponent} from './payment-accounts/edit-payment-account/edit-payment-account-dialog.component';
// tslint:disable-next-line: max-line-length
import { CreatePaymentCategoryDialogComponent } from "./payment-categories/create-payment-category/create-payment-category-dialog.component";
import { PaymentCategoriesComponent } from "./payment-categories/payment-categories.component";
import { EditPaymentCategoryDialogComponent } from "./payment-categories/edit-payment-category/edit-payment-category-dialog.component";
import { BsDatepickerModule, BsLocaleService } from "ngx-bootstrap/datepicker";
import { DropdownModule } from "primeng/dropdown";
import { RadioButtonModule } from 'primeng/radiobutton';
import { TableModule } from "primeng/table";
import { MultiSelectModule } from "primeng/multiselect";
import { AutoCompleteModule } from "primeng/autocomplete";
import { FileUploadModule } from "primeng/fileupload";
import { GalleriaModule } from "primeng/galleria";
import { DialogModule } from "primeng/dialog";
import { InputNumberModule } from "primeng/inputnumber";
import { MenuModule } from "primeng/menu";
import { ChartModule } from 'primeng/chart';
import { RichTextEditorAllModule } from '@syncfusion/ej2-angular-richtexteditor';

// tslint:disable-next-line: max-line-length
import {CreateHousingCategoryDialogComponent} from './housing-categories/create-housing-category/create-housing-category-dialog.component';
import {EditHousingCategoryDialogComponent} from './housing-categories/edit-housing-category/edit-housing-category-dialog.component';
import {HousingCategoriesComponent} from './housing-categories/housing-categories.component';
import {HousingPaymentPlanGroupsComponent} from './housing-payment-plan-groups/housing-payment-plan-groups.component';
import {
    CreateHousingPaymentPlanGroupDialogComponent
} from './housing-payment-plan-groups/create-housing-payment-plan-group/create-housing-payment-plan-group-dialog.component';
import {
    EditHousingPaymentPlanGroupDialogComponent
} from './housing-payment-plan-groups/edit-housing-payment-plan-group/edit-housing-payment-plan-group-dialog.component';
import {
    HousingPaymentPlanGroupAmountsComponent
} from './housing-payment-plan-groups/housing-payment-plan-group-amounts/housing-payment-plan-group-amounts.component';
import {PeriodsComponent} from './periods/periods.component';
import {CreatePeriodDialogComponent} from './periods/create-period/create-period-dialog.component';
import {EditPeriodDialogComponent} from './periods/edit-period/edit-period-dialog.component';
import {InventoryTypesComponent} from './inventory-types/inventory-types.component';
import {InventoriesComponent} from './inventories/inventories.component';
import {CreateInventoryTypeDialogComponent} from './inventory-types/create-inventory-type/create-inventory-type-dialog.component';
import {EditInventoryTypeDialogComponent} from './inventory-types/edit-inventory-type/edit-inventory-type-dialog.component';
import {CreateInventoryDialogComponent} from './inventories/create-inventory/create-inventory-dialog.component';
import {EditInventoryDialogComponent} from './inventories/edit-inventory/edit-inventory-dialog.component';
import {FinancialStatementsComponent} from './reports/financial-statements.component';


import {defineLocale} from 'ngx-bootstrap/chronos';
import {trLocale} from 'ngx-bootstrap/locale';

@NgModule({
    declarations: [
        AppComponent,
        HomeComponent,
        AboutComponent,
        // tenants
        TenantsComponent,
        CreateTenantDialogComponent,
        EditTenantDialogComponent,
        // roles
        RolesComponent,
        CreateRoleDialogComponent,
        EditRoleDialogComponent,
        // users
        UsersComponent,
        CreateUserDialogComponent,
        EditUserDialogComponent,
        ChangePasswordComponent,
        ResetPasswordDialogComponent,
        // layout
        HeaderComponent,
        HeaderLeftNavbarComponent,
        HeaderLanguageMenuComponent,
        HeaderUserMenuComponent,
        FooterComponent,
        SidebarComponent,
        SidebarLogoComponent,
        SidebarUserPanelComponent,
        SidebarMenuComponent,
        // custom-components
        HousingsComponent,
        CreateHousingDialogComponent,
        EditHousingDialogComponent,
        AddPersonDialogComponent,
        AccountActivitiesDialogComponent,
        HousingPeopleDialogComponent,
        BlocksComponent,
        CreateBlockDialogComponent,
        EditBlockDialogComponent,
        EmployeesComponent,
        CreateEmployeeDialogComponent,
        EditEmployeeDialogComponent,
        PeopleComponent,
        CreatePersonDialogComponent,
        EditPersonDialogComponent,
        AccountBooksComponent,
        PaymentAccountsComponent,
        CreatePaymentAccountDialogComponent,
        CreateHousingDialogComponent,
        EditPaymentAccountDialogComponent,
        CreatePaymentCategoryDialogComponent,
        EditPaymentCategoryDialogComponent,
        PaymentCategoriesComponent,
        CreateHousingCategoryDialogComponent,
        EditHousingCategoryDialogComponent,
        HousingCategoriesComponent,
        HousingPaymentPlanGroupsComponent,
        CreateHousingPaymentPlanGroupDialogComponent,
        EditHousingPaymentPlanGroupDialogComponent,
        HousingPaymentPlanGroupAmountsComponent,
        CreateAccountBookDialogComponent,
        EditAccountBookDialogComponent,
        PeriodsComponent,
        CreatePeriodDialogComponent,
        EditPeriodDialogComponent,
        InventoryTypesComponent,
        InventoriesComponent,
        CreateInventoryTypeDialogComponent,
        EditInventoryTypeDialogComponent,
        CreateInventoryDialogComponent,
        EditInventoryDialogComponent,
        FinancialStatementsComponent
    ],
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        HttpClientModule,
        HttpClientJsonpModule,
        ModalModule.forChild(),
        BsDropdownModule,
        CollapseModule,
        TabsModule,
        AppRoutingModule,
        ServiceProxyModule,
        SharedModule,
        NgxPaginationModule,
        BsDatepickerModule.forRoot(),
        DropdownModule,
        RadioButtonModule,
        TableModule,
        MultiSelectModule,
        InputNumberModule,
        AutoCompleteModule,
        FileUploadModule,
        GalleriaModule,
        DialogModule,
        MenuModule,
        ChartModule,
        RichTextEditorAllModule
    ],
    providers: [],
    entryComponents: [
        // tenants
        CreateTenantDialogComponent,
        EditTenantDialogComponent,
        // roles
        CreateRoleDialogComponent,
        EditRoleDialogComponent,
        // users
        CreateUserDialogComponent,
        EditUserDialogComponent,
        ResetPasswordDialogComponent,
    ],
})
export class AppModule {
    constructor(private bsLocaleService: BsLocaleService) {
        trLocale.invalidDate = 'Geçersiz tarih';
        defineLocale('tr', trLocale);
        this.bsLocaleService.use('tr');
    }
}
