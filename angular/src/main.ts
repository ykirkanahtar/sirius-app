import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { enableProdMode } from '@angular/core';
import { environment } from './environments/environment';
import { RootModule } from './root.module';
import { hmrBootstrap } from './hmr';
import { registerLicense } from '@syncfusion/ej2-base';

import 'moment/min/locales.min';
import 'moment-timezone';

// Registering Syncfusion license key
registerLicense('Mgo+DSMBaFt/QHJqVVhjWlpFdEBBXHxAd1p/VWJYdVt5flBPcDwsT3RfQF9jQX9XdkNnX31cdHJdQQ==;Mgo+DSMBPh8sVXJ0S0R+XE9HcFRDX3xKf0x/TGpQb19xflBPallYVBYiSV9jS3xTfkRnWXladnZXQmdVVg==;ORg4AjUWIQA/Gnt2VVhiQlFadVlJXGFWfVJpTGpQdk5xdV9DaVZUTWY/P1ZhSXxRdk1iXX9YdHJXRGRUWEM=;Njk3MTQ3QDMyMzAyZTMyMmUzMEdBc3NYdEVWeGxjMDFTd2tRNGlrM3c3bFBOTnlzMnNDR1pEcUFHUTRhSVU9;Njk3MTQ4QDMyMzAyZTMyMmUzMEJKdXhjR2xubnFqdnVuZEdrMHpLVlZzWVd3NXVyYk93aUxJUjU2SVhabVk9;NRAiBiAaIQQuGjN/V0Z+Xk9EaFxEVmJLYVB3WmpQdldgdVRMZVVbQX9PIiBoS35RdEVrWHtednBTRGJZVUZx;Njk3MTUwQDMyMzAyZTMyMmUzMGR5TkpJaU9UelFVNkNRcFN1QWpjdzZMYllUQm1vNFk1Nk4yN29nbUtMbjA9;Njk3MTUxQDMyMzAyZTMyMmUzMEtzV2JFRlpxaVlqeTJTU2VNWHRVbWpMbjNTS3BhR3JNNFBseUJnWC9JZjQ9;Mgo+DSMBMAY9C3t2VVhiQlFadVlJXGFWfVJpTGpQdk5xdV9DaVZUTWY/P1ZhSXxRdk1iXX9YdHJXRGVVV0Y=;Njk3MTUzQDMyMzAyZTMyMmUzMGJyY1NqZGkxWGRYM1IwZ1pYY3BQMEM4S2xDR0wzbG9MU3pFd3R1Q2x6dlU9;Njk3MTU0QDMyMzAyZTMyMmUzMEZEbjRDYi9KaVFxNDJqTFo1WCsrSlB0U1IyMDBBZXdia2o5Q1BRMzZHRmM9;Njk3MTU1QDMyMzAyZTMyMmUzMGR5TkpJaU9UelFVNkNRcFN1QWpjdzZMYllUQm1vNFk1Nk4yN29nbUtMbjA9');

if (environment.production) {
    enableProdMode();
}

const bootstrap = () => {
    return platformBrowserDynamic().bootstrapModule(RootModule);
};

/* "Hot Module Replacement" is enabled as described on
 * https://medium.com/@beeman/tutorial-enable-hrm-in-angular-cli-apps-1b0d13b80130#.sa87zkloh
 */

if (environment.hmr) {
    if (module['hot']) {
        hmrBootstrap(module, bootstrap); // HMR enabled bootstrap
    } else {
        console.error('HMR is not enabled for webpack-dev-server!');
        console.log('Are you using the --hmr flag for ng serve?');
    }
} else {
    bootstrap(); // Regular bootstrap
}
