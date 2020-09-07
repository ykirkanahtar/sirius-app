import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientJsonpModule } from '@angular/common/http';
import { HttpClientModule } from '@angular/common/http';
import { ModalModule } from 'ngx-bootstrap/modal';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { CollapseModule } from 'ngx-bootstrap/collapse';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { NgxPaginationModule } from 'ngx-pagination';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ServiceProxyModule } from '@shared/service-proxies/service-proxy.module';
import { NgModule } from '@angular/core';
import { SharedModule } from '@shared/shared.module';
import { HomeComponent } from '@app/home/home.component';
import { AboutComponent } from '@app/about/about.component';
// tenants
import { TenantsComponent } from '@app/tenants/tenants.component';
import { CreateTenantDialogComponent } from './tenants/create-tenant/create-tenant-dialog.component';
import { EditTenantDialogComponent } from './tenants/edit-tenant/edit-tenant-dialog.component';
// roles
import { RolesComponent } from '@app/roles/roles.component';
import { CreateRoleDialogComponent } from './roles/create-role/create-role-dialog.component';
import { EditRoleDialogComponent } from './roles/edit-role/edit-role-dialog.component';
// users
import { UsersComponent } from '@app/users/users.component';
import { CreateUserDialogComponent } from '@app/users/create-user/create-user-dialog.component';
import { EditUserDialogComponent } from '@app/users/edit-user/edit-user-dialog.component';
import { ChangePasswordComponent } from './users/change-password/change-password.component';
import { ResetPasswordDialogComponent } from './users/reset-password/reset-password.component';
// layout
import { HeaderComponent } from './layout/header.component';
import { HeaderLeftNavbarComponent } from './layout/header-left-navbar.component';
import { HeaderLanguageMenuComponent } from './layout/header-language-menu.component';
import { HeaderUserMenuComponent } from './layout/header-user-menu.component';
import { FooterComponent } from './layout/footer.component';
import { SidebarComponent } from './layout/sidebar.component';
import { SidebarLogoComponent } from './layout/sidebar-logo.component';
import { SidebarUserPanelComponent } from './layout/sidebar-user-panel.component';
import { SidebarMenuComponent } from './layout/sidebar-menu.component';

//custom-components
import { CreateHousingDialogComponent } from './housings/create-housing/create-housing-dialog.component';
import { EditHousingDialogComponent } from './housings/edit-housing/edit-housing-dialog.component';
import { EmployeesComponent } from './employees/employees.component';
import { CreateEmployeeDialogComponent } from './employees/create-employee/create-employee-dialog.component';
import { EditEmployeeDialogComponent } from './employees/edit-employee/edit-employee-dialog.component';
import { PeopleComponent } from './people/people.component';
import { CreatePersonDialogComponent } from './people/create-person/create-person-dialog.component';
import { EditPersonDialogComponent } from './people/edit-person/edit-person-dialog.component';
import { AccountBooksComponent } from './account-books/account-books.component';
import { PaymentAccountsComponent } from './payment-accounts/payment-accounts.component';
import { CreatePaymentAccountDialogComponent } from './payment-accounts/create-payment-account/create-payment-account-dialog.component';
import { CreateHousingDueAccountBookDialogComponent } from './account-books/create-account-book/create-housing-due-account-book-dialog.component';
import { HousingsComponent } from './housings/housings.component';
import { EditPaymentAccountDialogComponent } from './payment-accounts/edit-payment-account/edit-payment-account-dialog.component';
import { CreatePaymentCategoryDialogComponent } from './payment-categories/create-payment-category/create-payment-category-dialog.component';
import { PaymentCategoriesComponent } from './payment-categories/payment-categories.component';
import { EditPaymentCategoryDialogComponent } from './payment-categories/edit-payment-category/edit-payment-category-dialog.component';


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
    CreateHousingDueAccountBookDialogComponent,
    CreatePaymentCategoryDialogComponent,
    EditPaymentCategoryDialogComponent,
    PaymentCategoriesComponent
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
export class AppModule {}
