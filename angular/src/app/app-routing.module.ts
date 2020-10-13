import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AppComponent } from './app.component';
import { AppRouteGuard } from '@shared/auth/auth-route-guard';
import { HomeComponent } from './home/home.component';
import { AboutComponent } from './about/about.component';
import { UsersComponent } from './users/users.component';
import { TenantsComponent } from './tenants/tenants.component';
import { RolesComponent } from 'app/roles/roles.component';
import { ChangePasswordComponent } from './users/change-password/change-password.component';
import { HousingsComponent } from 'app/housings/housings.component';
import { EmployeesComponent } from './employees/employees.component';
import { PeopleComponent } from './people/people.component';
import { AccountBooksComponent } from './account-books/account-books.component';
import { PaymentAccountsComponent } from './payment-accounts/payment-accounts.component';
import { PaymentCategoriesComponent } from './payment-categories/payment-categories.component';
import { HousingCategoriesComponent } from './housing-categories/housing-categories.component';
import { CreateHousingPaymentPlanComponent } from './housing-payment-plans/create-housing-payment-plan/create-housing-payment-plan.component';
@NgModule({
    imports: [
        RouterModule.forChild([
            {
                path: '',
                component: AppComponent,
                children: [
                    { path: 'home', component: HomeComponent,  canActivate: [AppRouteGuard] },
                    { path: 'account-books', component: AccountBooksComponent, data: { permission: 'Pages.Users' }, canActivate: [AppRouteGuard] },                                        
                    { path: 'housings', component: HousingsComponent, data: { permission: 'Pages.Users' }, canActivate: [AppRouteGuard] },                    
                    { path: 'payment-accounts', component: PaymentAccountsComponent, data: { permission: 'Pages.Users' }, canActivate: [AppRouteGuard] },                    
                    { path: 'employees', component: EmployeesComponent, data: { permission: 'Pages.Users' }, canActivate: [AppRouteGuard] },                    
                    { path: 'people', component: PeopleComponent, data: { permission: 'Pages.Users' }, canActivate: [AppRouteGuard] },                    
                    { path: 'account-books', component: AccountBooksComponent, data: { permission: 'Pages.Users' }, canActivate: [AppRouteGuard] },                    
                    { path: 'payment-categories', component: PaymentCategoriesComponent, data: { permission: 'Pages.Users' }, canActivate: [AppRouteGuard] },                    
                    { path: 'housing-categories', component: HousingCategoriesComponent, data: { permission: 'Pages.Users' }, canActivate: [AppRouteGuard] }, 
                    { path: 'create-housing-payment-plans', component: CreateHousingPaymentPlanComponent, data: { permission: 'Pages.Users' }, canActivate: [AppRouteGuard] },                    
                    { path: 'users', component: UsersComponent, data: { permission: 'Pages.Users' }, canActivate: [AppRouteGuard] },
                    { path: 'roles', component: RolesComponent, data: { permission: 'Pages.Roles' }, canActivate: [AppRouteGuard] },
                    { path: 'tenants', component: TenantsComponent, data: { permission: 'Pages.Tenants' }, canActivate: [AppRouteGuard] },
                    { path: 'about', component: AboutComponent },
                    { path: 'update-password', component: ChangePasswordComponent }
                ]
            }
        ])
    ],
    exports: [RouterModule]
})
export class AppRoutingModule { }
