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
  PaymentPlanForHousingCategoryDto,
} from "@shared/service-proxies/service-proxies";
import * as moment from "moment";
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
  paymentAccounts: LookUpDto[];
  residentOrOwners: any[];
  residentOrOwnerEnum = ResidentOrOwner;
  startDate: Date;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _housingPaymentPlanGroupService: HousingPaymentPlanGroupServiceProxy,
    private _housingCategoryService: HousingCategoryServiceProxy,
    private _paymentAccountService: PaymentAccountServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._housingCategoryService
      .getHousingCategoryLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.housingCategories = result;

        this.input.paymentPlanForHousingCategories = [];

        this.housingCategories.forEach( (currentValue, index) => {
          let housingPaymentPlanForHousingCategory = new PaymentPlanForHousingCategoryDto();
          housingPaymentPlanForHousingCategory.housingCategoryId = currentValue.value;
          housingPaymentPlanForHousingCategory.amountPerMonth = 0;
          this.input.paymentPlanForHousingCategories.push(housingPaymentPlanForHousingCategory);
        });

      });

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
    return this.housingCategories.filter(p => p.value === housingCategoryId).map(p => p.label)[0];
  }

  save(): void {
    this.saving = true;

    this.input.startDateString = CommonFunctions.dateToString(this.startDate);

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
