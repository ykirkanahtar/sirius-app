import {
  Component,
  Injector,
  OnInit,
  EventEmitter,
  Output,
} from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef } from 'ngx-bootstrap/modal';
import * as _ from 'lodash';
import { AppComponentBase } from '@shared/app-component-base';
import { PaymentCategoryDto, PaymentCategoryServiceProxy } from '@shared/service-proxies/service-proxies';

@Component({
  templateUrl: 'edit-payment-category-dialog.component.html'
})
export class EditPaymentCategoryDialogComponent extends AppComponentBase
  implements OnInit {
  saving = false;
  id: string;
  paymentCategory = new PaymentCategoryDto();

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _paymentCategoryService: PaymentCategoryServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._paymentCategoryService
      .get(this.id)
      .subscribe((result: PaymentCategoryDto) => {
          this.paymentCategory = result;
      });
  }

  save(): void {
    this.saving = true;

    const paymentCategory = new PaymentCategoryDto();
    paymentCategory.init(this.paymentCategory);

    this._paymentCategoryService
      .update(paymentCategory)
      .pipe(
        finalize(() => {
          this.saving = false;
        })
      )
      .subscribe(() => {
        this.notify.info(this.l('SavedSuccessfully'));
        this.bsModalRef.hide();
        this.onSave.emit();
      });
  }
}
