import {
  Component,
  Injector,
  OnInit,
  EventEmitter,
  Output,
} from "@angular/core";
import { finalize } from "rxjs/operators";
import { BsModalRef } from "ngx-bootstrap/modal";
import * as _ from "lodash";
import { AppComponentBase } from "@shared/app-component-base";
import {
  UpdateHousingPaymentPlanGroupDto,
  HousingPaymentPlanGroupServiceProxy,
  HousingPaymentPlanGroupDto,
  LookUpDto,
  PaymentAccountServiceProxy,
} from "@shared/service-proxies/service-proxies";

@Component({
  templateUrl: "./edit-housing-payment-plan-group-dialog.component.html",
})
export class EditHousingPaymentPlanGroupDialogComponent
  extends AppComponentBase
  implements OnInit {
  saving = false;
  id: string;
  input = new UpdateHousingPaymentPlanGroupDto();
  housingPaymentPlanGroup = new HousingPaymentPlanGroupDto();
  paymentAccounts: LookUpDto[];

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _housingPaymentPlanGroupService: HousingPaymentPlanGroupServiceProxy,
    private _paymentAccountService: PaymentAccountServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._housingPaymentPlanGroupService
      .getForUpdate(this.id)
      .subscribe((result: UpdateHousingPaymentPlanGroupDto) => {
        this.input = result;
      });

    this._paymentAccountService
      .getPaymentAccountLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.paymentAccounts = result;
      });
  }

  save(): void {
    this.saving = true;


    this._housingPaymentPlanGroupService
      .update(this.input)
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
