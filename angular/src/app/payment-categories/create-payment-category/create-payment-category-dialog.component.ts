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
  LookUpDto,
  PaymentAccountServiceProxy,
  PaymentCategoryType,
  CreateIncomePaymentCategoryDto,
  CreateExpensePaymentCategoryDto,
  CreateTransferPaymentCategoryDto,
  HousingDueType,
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
  paymentCategoryType: PaymentCategoryType;
  PaymentCategoryTypeEnum = PaymentCategoryType;
  HousingDueTypeEnum = HousingDueType;
  title: string;
  housingDue: boolean;

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

    if(this.paymentCategoryType === PaymentCategoryType.Income) {
      this.title = this.l("CreateNewIncomePaymentCategory");
    } else if(this.paymentCategoryType === PaymentCategoryType.Expense) {
      this.title = this.l("CreateNewExpensePaymentCategory");
    } else if(this.paymentCategoryType === PaymentCategoryType.TransferBetweenAccounts) {
      this.title = this.l("CreateNewTransferPaymentCategory");
    }

    this._paymentAccountServiceProxy
      .getPaymentAccountLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.paymentAccounts = result;
      });
  }

  save(): void {
    this.saving = true;

    if (this.paymentCategoryType === PaymentCategoryType.Income) {
      const paymentCategory = new CreateIncomePaymentCategoryDto();
      paymentCategory.init(this.paymentCategory);

      this._paymentCategoryService
        .createIncome(paymentCategory)
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
    } else if (this.paymentCategoryType === PaymentCategoryType.Expense) {
      const paymentCategory = new CreateExpensePaymentCategoryDto();
      paymentCategory.init(this.paymentCategory);

      this._paymentCategoryService
        .createExpense(paymentCategory)
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
    } else {
      const paymentCategory = new CreateTransferPaymentCategoryDto();
      paymentCategory.init(this.paymentCategory);

      this._paymentCategoryService
        .createTransfer(paymentCategory)
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
}
