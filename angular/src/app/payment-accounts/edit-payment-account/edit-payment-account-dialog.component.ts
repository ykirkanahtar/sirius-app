import {
  Component,
  Injector,
  OnInit,
  EventEmitter,
  Output,
  Input,
} from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef } from 'ngx-bootstrap/modal';
import * as _ from 'lodash';
import { AppComponentBase } from '@shared/app-component-base';
import { PaymentAccountDto, PaymentAccountServiceProxy, PaymentAccountType } from '@shared/service-proxies/service-proxies';

@Component({
  templateUrl: 'edit-payment-account-dialog.component.html'
})
export class EditPaymentAccountDialogComponent extends AppComponentBase
  implements OnInit {
  saving = false;
  id: string;
  paymentAccount = new PaymentAccountDto();
  paymentAccountTypeEnum = PaymentAccountType;

  @Input() paymentAccountType: PaymentAccountType;
  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _paymentAccountService: PaymentAccountServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._paymentAccountService
      .get(this.id)
      .subscribe((result: PaymentAccountDto) => {
        this.paymentAccount = result;
      });
  }

  save(): void {
    this.saving = true;

    const paymentAccount = new PaymentAccountDto();
    paymentAccount.init(this.paymentAccount);

    this._paymentAccountService
      .update(paymentAccount)
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
