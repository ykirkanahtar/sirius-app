import {
  Component,
  Injector,
  OnInit,
  EventEmitter,
  Output
} from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef } from 'ngx-bootstrap/modal';
import * as _ from 'lodash';
import { AppComponentBase } from '@shared/app-component-base';
import {
  AccountBookDto,
  AccountBookServiceProxy,
  HousingServiceProxy,
  PaymentAccountServiceProxy,
  LookUpDto,
  PaymentCategoryServiceProxy,
  CreateOtherPaymentAccountBookDto
} from '@shared/service-proxies/service-proxies';

@Component({
  templateUrl: 'create-other-payment-account-book-dialog.component.html'
})
export class CreateOtherPaymentAccountBookDialogComponent extends AppComponentBase
  implements OnInit {
  saving = false;
  accountBook = new AccountBookDto();

  housings: LookUpDto[];
  paymentAccounts: LookUpDto[];
  paymentCategories: LookUpDto[];

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _accountBookServiceProxy: AccountBookServiceProxy,
    private _housingServiceProxy: HousingServiceProxy,
    private _paymentAccountServiceProxy: PaymentAccountServiceProxy,
    private _paymentCategoryServiceProxy: PaymentCategoryServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._housingServiceProxy
      .getHousingLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.housings = result;
      });

    this._paymentAccountServiceProxy
      .getPaymentAccountLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.paymentAccounts = result;
      });

    this._paymentCategoryServiceProxy
      .getPaymentCategoryLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.paymentCategories = result;
      });
  }

  save(): void {
    this.saving = true;

    const accountBook = new CreateOtherPaymentAccountBookDto();
    accountBook.init(this.accountBook);

    this._accountBookServiceProxy
      .createOtherPayment(accountBook)
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
