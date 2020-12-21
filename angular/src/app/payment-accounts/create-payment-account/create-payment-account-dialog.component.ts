import {
  Component,
  Injector,
  OnInit,
  EventEmitter,
  Output,
  Input
} from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef } from 'ngx-bootstrap/modal';
import * as _ from 'lodash';
import { AppComponentBase } from '@shared/app-component-base';
import { 
  PaymentAccountDto, 
  PaymentAccountServiceProxy, 
  CreateCashAccountDto, 
  CreateBankOrAdvanceAccountDto, 
  PaymentAccountType,
  CreateTransferForPaymentAccountDto,
} from '@shared/service-proxies/service-proxies';
import * as moment from 'moment';

@Component({
  templateUrl: 'create-payment-account-dialog.component.html'
})
export class CreatePaymentAccountDialogComponent extends AppComponentBase
  implements OnInit {
  saving = false;
  paymentAccount = new PaymentAccountDto();
  transferForPaymentAccount = new CreateTransferForPaymentAccountDto();

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
    this.paymentAccount.tenantIsOwner = true;
  }

  save(): void {
    this.saving = true;

    if (this.paymentAccountType === PaymentAccountType.Cash) {
      const cashPaymentAccount = new CreateCashAccountDto();
      cashPaymentAccount.init(this.paymentAccount);
      cashPaymentAccount.createTransferForPaymentAccount = this.transferForPaymentAccount;

      this._paymentAccountService
        .createCashAccount(cashPaymentAccount)
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
    } else if (this.paymentAccountType === PaymentAccountType.BankAccount) {
      const bankAccountPaymentAccount = new CreateBankOrAdvanceAccountDto();
      bankAccountPaymentAccount.init(this.paymentAccount);
      bankAccountPaymentAccount.createTransferForPaymentAccount = this.transferForPaymentAccount;

      this._paymentAccountService
        .createBankAccount(bankAccountPaymentAccount)
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
    } else if (this.paymentAccountType === PaymentAccountType.AdvanceAccount) {
      const advanceAccountPaymentAccount = new CreateBankOrAdvanceAccountDto();
      advanceAccountPaymentAccount.init(this.paymentAccount);
      
      this._paymentAccountService
        .createAdvanceAccount(advanceAccountPaymentAccount)
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
}
