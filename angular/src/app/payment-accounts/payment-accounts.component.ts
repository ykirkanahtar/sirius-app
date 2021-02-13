import { Component, Injector, OnInit, ViewChild } from "@angular/core";
import { finalize } from "rxjs/operators";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { appModuleAnimation } from "@shared/animations/routerTransition";
import {
  PagedListingComponentBase,
  PagedRequestDto,
} from "@shared/paged-listing-component-base";
import { CreatePaymentAccountDialogComponent } from "./create-payment-account/create-payment-account-dialog.component";
import {
  PaymentAccountDto,
  PaymentAccountServiceProxy,
  PaymentAccountDtoPagedResultDto,
  PaymentAccountType,
} from "@shared/service-proxies/service-proxies";
import { LazyLoadEvent } from "primeng/api";
import { Table } from "primeng/table";
import { EditPaymentAccountDialogComponent } from "./edit-payment-account/edit-payment-account-dialog.component";
import { MenuItem as PrimeNgMenuItem } from "primeng/api";

class PagedPaymentAccountRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: "./payment-accounts.component.html",
  animations: [appModuleAnimation()],
})
export class PaymentAccountsComponent
  extends PagedListingComponentBase<PaymentAccountDto>
  implements OnInit {
  @ViewChild("dataTable", { static: true }) dataTable: Table;

  sortingColumn: string;
  items: PrimeNgMenuItem[];

  paymentAccounts: PaymentAccountDto[] = [];

  paymentAccountTypeEnum = PaymentAccountType;

  constructor(
    injector: Injector,
    private _paymentAccountService: PaymentAccountServiceProxy,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.items = [
      {
        label: this.l("CashAccount"),
        icon: "pi pi-money-bill",
        command: () => {
          this.createPaymentAccount(this.paymentAccountTypeEnum.Cash);
        },
      },
      {
        label: this.l("BankAccount"),
        icon: "pi pi-home",
        command: () => {
          this.createPaymentAccount(this.paymentAccountTypeEnum.BankAccount);
        },
      },
      {
        label: this.l("AdvanceAccount"),
        icon: "pi pi-forward",
        command: () => {
          this.createPaymentAccount(this.paymentAccountTypeEnum.AdvanceAccount);
        },
      },
    ];

    this.getDataPage(1);
  }

  createPaymentAccount(paymentAccountType: PaymentAccountType): void {
    this.showCreatePaymentAccountDialog(paymentAccountType);
  }

  editPaymentAccount(paymentAccount: PaymentAccountDto): void {
    this.showEditPaymentAccountDialog(paymentAccount);
  }

  getData(event?: LazyLoadEvent) {
    this.sortingColumn = this.primengTableHelper.getSorting(this.dataTable);
    this.getDataPage(1);
  }

  protected list(
    request: PagedPaymentAccountRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    this._paymentAccountService
      .getAll(this.sortingColumn, request.skipCount, request.maxResultCount)
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: PaymentAccountDtoPagedResultDto) => {
        this.paymentAccounts = result.items;
        this.showPaging(result, pageNumber);
      });
  }

  protected delete(paymentAccount: PaymentAccountDto): void {
    abp.message.confirm(
      this.l("PaymentAccountDeleteWarningMessage", paymentAccount.accountName),
      undefined,
      (result: boolean) => {
        if (result) {
          this._paymentAccountService
            .delete(paymentAccount.id)
            .subscribe(() => {
              abp.notify.success(this.l("SuccessfullyDeleted"));
              this.refresh();
            });
        }
      }
    );
  }

  private showCreatePaymentAccountDialog(
    paymentAccountType: PaymentAccountType
  ): void {
    let createOrEditPaymentAccountDialog: BsModalRef;
    createOrEditPaymentAccountDialog = this._modalService.show(
      CreatePaymentAccountDialogComponent,
      {
        class: "modal-lg",
      }
    );

    createOrEditPaymentAccountDialog.content.paymentAccountType = paymentAccountType;

    createOrEditPaymentAccountDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  private showEditPaymentAccountDialog(
    paymentAccount: PaymentAccountDto
  ): void {
    let createOrEditPaymentAccountDialog: BsModalRef;

    createOrEditPaymentAccountDialog = this._modalService.show(
      EditPaymentAccountDialogComponent,
      {
        class: "modal-lg",
        initialState: {
          id: paymentAccount.id,
        },
      }
    );

    createOrEditPaymentAccountDialog.content.paymentAccountType =
      paymentAccount.paymentAccountType;

    createOrEditPaymentAccountDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }
}
