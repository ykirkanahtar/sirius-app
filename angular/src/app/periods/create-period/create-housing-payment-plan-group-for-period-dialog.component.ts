import {
  Component,
  Injector,
  OnInit,
  EventEmitter,
  Output,
} from "@angular/core";
import * as _ from "lodash";
import { AppComponentBase } from "@shared/app-component-base";

import {
  CreateHousingPaymentPlanGroupDto,
  HousingCategoryServiceProxy,
  LookUpDto,
} from "@shared/service-proxies/service-proxies";
import { BsModalRef } from "ngx-bootstrap/modal";
import { Moment } from "moment";

@Component({
  selector: "createHousingPaymentPlanGroupForPeriod",
  templateUrl:
    "./create-housing-payment-plan-group-for-period-dialog.component.html",
})
export class CreateHousingPaymentPlanGroupForPeriodDialogComponent
  extends AppComponentBase
  implements OnInit {
  saving = false;
  input = new CreateHousingPaymentPlanGroupDto();
  housingCategories: LookUpDto[];

  periodStartDate: Moment;

  @Output() onSave = new EventEmitter<CreateHousingPaymentPlanGroupDto>();

  constructor(
    injector: Injector,
    private _housingCategoryService: HousingCategoryServiceProxy,
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

    if (this.periodStartDate) {
      this.input.startDate = this.periodStartDate;
    }
  }

  save(): void {
    this.saving = true;

    if(this.periodStartDate && this.input.startDate < this.periodStartDate) {
      abp.message.error(this.l("HousingPaymentPlanGroupStartDateCanNotBeSmallThanPeriodStartDate"), this.l("Error"));
      this.saving = false;
      return;
    }

    this.notify.info(this.l("AddedSuccessfully"));
    this.bsModalRef.hide();
    this.onSave.emit(this.input);
    this.saving = false;
  }
}
