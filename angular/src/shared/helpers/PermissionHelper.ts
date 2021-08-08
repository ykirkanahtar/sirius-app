
import { PermissionCheckerService } from 'abp-ng2-module';

import { Injectable } from '@angular/core';

@Injectable()
export class PermissionHelper {

    constructor(
        private _permissionCheckerService: PermissionCheckerService) {
    }

    createAccountBook(): boolean {
        return this._permissionCheckerService.isGranted('Pages.CreateAccountBook');
    }

    editAccountBook(): boolean {
        return this._permissionCheckerService.isGranted('Pages.EditAccountBook');
    }

    deleteAccountBook(): boolean {
        return this._permissionCheckerService.isGranted('Pages.DeleteAccountBook');
    }

    createBlock(): boolean {
        return this._permissionCheckerService.isGranted('Pages.CreateBlock');
    }

    editBlock(): boolean {
        return this._permissionCheckerService.isGranted('Pages.EditBlock');
    }

    deleteBlock(): boolean {
        return this._permissionCheckerService.isGranted('Pages.DeleteBlock');
    }

    createEmployee(): boolean {
        return this._permissionCheckerService.isGranted('Pages.CreateEmployee');
    }

    editEmployee(): boolean {
        return this._permissionCheckerService.isGranted('Pages.EditEmployee');
    }

    deleteEmployee(): boolean {
        return this._permissionCheckerService.isGranted('Pages.DeleteEmployee');
    }

    createHousingCategory(): boolean {
        return this._permissionCheckerService.isGranted('Pages.CreateHousingCategory');
    }

    editHousingCategory(): boolean {
        return this._permissionCheckerService.isGranted('Pages.EditHousingCategory');
    }

    deleteHousingCategory(): boolean {
        return this._permissionCheckerService.isGranted('Pages.DeleteHousingCategory');
    }

    createHousingPaymentPlanGroup(): boolean {
        return this._permissionCheckerService.isGranted('Pages.CreateHousingPaymentPlanGroup');
    }

    editHousingPaymentPlanGroup(): boolean {
        return this._permissionCheckerService.isGranted('Pages.EditHousingPaymentPlanGroup');
    }

    deleteHousingPaymentPlanGroup(): boolean {
        return this._permissionCheckerService.isGranted('Pages.DeleteHousingPaymentPlanGroup');
    }

    createHousing(): boolean {
        return this._permissionCheckerService.isGranted('Pages.CreateHousing');
    }

    editHousing(): boolean {
        return this._permissionCheckerService.isGranted('Pages.EditHousing');
    }

    deleteHousing(): boolean {
        return this._permissionCheckerService.isGranted('Pages.DeleteHousing');
    }

    addPersonToHousing(): boolean {
        return this._permissionCheckerService.isGranted('Pages.AddPersonToHousing');
    }

    createPaymentAccount(): boolean {
        return this._permissionCheckerService.isGranted('Pages.CreatePaymentAccount');
    }

    editPaymentAccount(): boolean {
        return this._permissionCheckerService.isGranted('Pages.EditPaymentAccount');
    }

    deletePaymentAccount(): boolean {
        return this._permissionCheckerService.isGranted('Pages.DeletePaymentAccount');
    }

    createPaymentCategory(): boolean {
        return this._permissionCheckerService.isGranted('Pages.CreatePaymentCategory');
    }

    editPaymentCategory(): boolean {
        return this._permissionCheckerService.isGranted('Pages.EditPaymentCategory');
    }

    deletePaymentCategory(): boolean {
        return this._permissionCheckerService.isGranted('Pages.DeletePaymentCategory');
    }

    createPerson(): boolean {
        return this._permissionCheckerService.isGranted('Pages.CreatePerson');
    }

    editPerson(): boolean {
        return this._permissionCheckerService.isGranted('Pages.EditPerson');
    }

    deletePerson(): boolean {
        return this._permissionCheckerService.isGranted('Pages.DeletePerson');
    }

    createPeriodForSite(): boolean {
        return this._permissionCheckerService.isGranted('Pages.CreatePeriodForSite');
    }

    createPeriodForBlock(): boolean {
        return this._permissionCheckerService.isGranted('Pages.CreatePeriodForBlock');
    }

    editPeriod(): boolean {
        return this._permissionCheckerService.isGranted('Pages.EditPeriod');
    }

    deletePeriod(): boolean {
        return this._permissionCheckerService.isGranted('Pages.DeletePeriod');
    }


    createInventoryType(): boolean {
        return this._permissionCheckerService.isGranted('Pages.CreateInventoryType');
    }

    editInventoryType(): boolean {
        return this._permissionCheckerService.isGranted('Pages.EditInventoryType');
    }

    deleteInventoryType(): boolean {
        return this._permissionCheckerService.isGranted('Pages.DeleteInventoryType');
    }

    createInventory(): boolean {
        return this._permissionCheckerService.isGranted('Pages.CreateInventory');
    }

    editInventory(): boolean {
        return this._permissionCheckerService.isGranted('Pages.EditInventory');
    }

    deleteInventory(): boolean {
        return this._permissionCheckerService.isGranted('Pages.DeleteInventory');
    }
}

