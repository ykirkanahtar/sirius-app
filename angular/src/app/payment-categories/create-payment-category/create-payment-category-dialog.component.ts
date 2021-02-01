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
  PaymentCategoryDto,
  PaymentCategoryServiceProxy,
  CreatePaymentCategoryDto,
  LookUpDto,
  PaymentAccountServiceProxy,
} from "@shared/service-proxies/service-proxies";

@Component({
  templateUrl: "create-payment-category-dialog.component.html",
})
export class CreatePaymentCategoryDialogComponent
  extends AppComponentBase
  implements OnInit {
  saving = false;
  paymentCategory = new PaymentCategoryDto();
  paymentAccounts: LookUpDto[];

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _paymentCategoryService: PaymentCategoryServiceProxy,
    private _paymentAccountServiceProxy: PaymentAccountServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._paymentAccountServiceProxy
      .getPaymentAccountLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.paymentAccounts = result;
      });
  }

  save(): void {
    this.saving = true;

    const paymentCategory = new CreatePaymentCategoryDto();
    paymentCategory.init(this.paymentCategory);

    this._paymentCategoryService
      .create(paymentCategory)
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
