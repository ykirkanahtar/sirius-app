import {
  Component,
  Injector,
  OnInit,
  EventEmitter,
  Output,
} from "@angular/core";
import { finalize } from "rxjs/operators";
import * as _ from "lodash";
import { AppComponentBase } from "@shared/app-component-base";

import {
  CreateHousingPaymentPlanGroupDto,
  HousingCategoryServiceProxy,
  LookUpDto,
  HousingPaymentPlanGroupServiceProxy,
  PaymentAccountServiceProxy,
  ResidentOrOwner,
  HousingPaymentPlanGroupForHousingCategoryDto,
  HousingServiceProxy,
  HousingPaymentPlanGroupForHousingDto,
} from "@shared/service-proxies/service-proxies";
import { BsModalRef } from "ngx-bootstrap/modal";
import { CommonFunctions } from "@shared/helpers/CommonFunctions";

@Component({
  templateUrl: "./create-housing-payment-plan-group-dialog.component.html",
})
export class CreateHousingPaymentPlanGroupDialogComponent
  extends AppComponentBase
  implements OnInit {
  saving = false;
  input = new CreateHousingPaymentPlanGroupDto();
  housingCategories: LookUpDto[];
  housings: LookUpDto[];
  selectedHousings: string[];
  amountPerMonthForHousing: number;
  paymentAccounts: LookUpDto[];
  residentOrOwners: any[];
  residentOrOwnerEnum = ResidentOrOwner;
  startDate: Date;
  showCategory: boolean;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _housingPaymentPlanGroupService: HousingPaymentPlanGroupServiceProxy,
    private _housingCategoryService: HousingCategoryServiceProxy,
    private _paymentAccountService: PaymentAccountServiceProxy,
    private _housingService: HousingServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    if (this.showCategory) {
      this._housingCategoryService
        .getHousingCategoryLookUp()
        .subscribe((result: LookUpDto[]) => {
          this.housingCategories = result;

          this.input.housingPaymentPlanGroupForHousingCategories = [];

          this.housingCategories.forEach((currentValue, index) => {
            let housingPaymentPlanForHousingCategory = new HousingPaymentPlanGroupForHousingCategoryDto();
            housingPaymentPlanForHousingCategory.housingCategoryId =
              currentValue.value;
            housingPaymentPlanForHousingCategory.amountPerMonth = 0;
            this.input.housingPaymentPlanGroupForHousingCategories.push(
              housingPaymentPlanForHousingCategory
            );
          });
        });
    } else {
      this._housingService
        .getHousingLookUp(undefined, undefined)
        .subscribe((result: LookUpDto[]) => {
          this.housings = result;
        });
    }

    this._paymentAccountService
      .getPaymentAccountLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.paymentAccounts = result;
      });

    this.residentOrOwners = [
      {
        value: ResidentOrOwner.Resident.toString(),
        label: this.l(this.residentOrOwnerEnum[ResidentOrOwner.Resident]),
      },
      {
        value: ResidentOrOwner.Owner.toString(),
        label: this.l(this.residentOrOwnerEnum[ResidentOrOwner.Owner]),
      },
    ];
  }

  getHousingCategoryName(housingCategoryId: string): string {
    return this.housingCategories
      .filter((p) => p.value === housingCategoryId)
      .map((p) => p.label)[0];
  }

  save(): void {
    this.saving = true;

    this.input.startDateString = CommonFunctions.dateToString(this.startDate);

    if(this.showCategory === false) {
      this.input.housingPaymentPlanGroupForHousings = [];

      this.selectedHousings.forEach((currentValue, index) => {
        let housingPaymentPlanForHousing = new HousingPaymentPlanGroupForHousingDto();
        housingPaymentPlanForHousing.housingId = currentValue;
        housingPaymentPlanForHousing.amountPerMonth = this.amountPerMonthForHousing;
        this.input.housingPaymentPlanGroupForHousings.push(
          housingPaymentPlanForHousing
        );
      });
    }

    this._housingPaymentPlanGroupService
      .create(this.input)
      .pipe(
        finalize(() => {
          this.saving = false;
        })
      )
      .subscribe(() => {
        this.notify.info(this.l("SavedSuccessfully"));
        this.bsModalRef.hide();
        this.onSave.emit();
      });
  }
}
