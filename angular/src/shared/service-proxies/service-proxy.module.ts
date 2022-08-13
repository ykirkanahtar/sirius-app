import { NgModule } from '@angular/core';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { AbpHttpInterceptor } from 'abp-ng2-module';

import * as ApiServiceProxies from './service-proxies';
import * as CustomApiServiceProxies from './custom-service-proxies';



@NgModule({
    providers: [
        ApiServiceProxies.RoleServiceProxy,
        ApiServiceProxies.SessionServiceProxy,
        ApiServiceProxies.TenantServiceProxy,
        ApiServiceProxies.UserServiceProxy,
        ApiServiceProxies.TokenAuthServiceProxy,
        ApiServiceProxies.AccountServiceProxy,
        ApiServiceProxies.ConfigurationServiceProxy,
        ApiServiceProxies.HousingServiceProxy,
        ApiServiceProxies.EmployeeServiceProxy,
        ApiServiceProxies.PersonServiceProxy,
        ApiServiceProxies.HousingPaymentPlanServiceProxy,
        ApiServiceProxies.HousingPaymentPlanGroupServiceProxy,
        ApiServiceProxies.AccountBookServiceProxy,
        ApiServiceProxies.PaymentAccountServiceProxy,
        ApiServiceProxies.PaymentCategoryServiceProxy,
        ApiServiceProxies.HousingCategoryServiceProxy,
        ApiServiceProxies.BlockServiceProxy,
        ApiServiceProxies.UploadServiceProxy,
        ApiServiceProxies.PeriodServiceProxy,
        ApiServiceProxies.ReportServiceProxy,
        ApiServiceProxies.InventoryTypeServiceProxy,
        ApiServiceProxies.InventoryServiceProxy,
        CustomApiServiceProxies.CustomUploadServiceProxy,
        { provide: HTTP_INTERCEPTORS, useClass: AbpHttpInterceptor, multi: true }
    ]
})
export class ServiceProxyModule { }
