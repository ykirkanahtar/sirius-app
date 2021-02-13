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
  PaymentCategoryServiceProxy,
  PaymentCategoryDto,
} from "@shared/service-proxies/service-proxies";
import * as moment from "moment";
import { BsModalRef } from "ngx-bootstrap/modal";

@Component({
  templateUrl: "./create-housing-payment-plan-group-dialog.component.html",
})
export class CreateHousingPaymentPlanGroupDialogComponent
  extends AppComponentBase
  implements OnInit {
  saving = false;
  input = new CreateHousingPaymentPlanGroupDto();
  housingCategories: LookUpDto[];
  paymentCategories: LookUpDto[];

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _housingPaymentPlanGroupService: HousingPaymentPlanGroupServiceProxy,
    private _housingCategoryService: HousingCategoryServiceProxy,
    private _paymentCategoryService: PaymentCategoryServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._housingCategoryService
      .getHousingCategoryLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.housingCategories = result;
      });

    this._paymentCategoryService
      .getHousingDuePaymentCategoryLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.paymentCategories = result;
      });

    this._paymentCategoryService
      .getRegularHousingDue()
      .subscribe((result: PaymentCategoryDto) => {
        this.input.paymentCategoryId = result.id;
      });
  }

  save(): void {
    this.saving = true;

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
