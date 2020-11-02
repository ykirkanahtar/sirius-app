import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {
  PagedListingComponentBase,
  PagedRequestDto
} from '@shared/paged-listing-component-base';
import { CreatePaymentAccountDialogComponent } from './create-payment-account/create-payment-account-dialog.component';
import { PaymentAccountDto, PaymentAccountServiceProxy, PaymentAccountDtoPagedResultDto, PaymentAccountType } from '@shared/service-proxies/service-proxies';
import { LazyLoadEvent } from 'primeng/api';
import { Table } from 'primeng/table';

class PagedPaymentAccountRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: './payment-accounts.component.html',
  animations: [appModuleAnimation()]
})
export class PaymentAccountsComponent extends PagedListingComponentBase<PaymentAccountDto>
  implements OnInit {

  @ViewChild('dataTable', { static: true }) dataTable: Table;

  sortingColumn: string;
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
    this.getDataPage(1);
  }

  createPaymentAccount(paymentAccountType: PaymentAccountType): void {
    this.showCreateOrEditPaymentAccountDialog(paymentAccountType);
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
      this.l('PaymentAccountDeleteWarningMessage', paymentAccount.accountName),
      undefined,
      (result: boolean) => {
        if (result) {
          this._paymentAccountService
            .delete(paymentAccount.id)
            .pipe(
              finalize(() => {
                abp.notify.success(this.l('SuccessfullyDeleted'));
                this.refresh();
              })
            )
            .subscribe(() => { });
        }
      }
    );
  }



  private showCreateOrEditPaymentAccountDialog(paymentAccountType: PaymentAccountType, id?: number): void {
    let createOrEditPaymentAccountDialog: BsModalRef;
    if (!id) {
      createOrEditPaymentAccountDialog = this._modalService.show(
        CreatePaymentAccountDialogComponent,
        {
          class: 'modal-lg',
        }
      );
    }
    // else {
    //   createOrEditPaymentAccountDialog = this._modalService.show(
    //     EditPaymentAccountDialogComponent,
    //     {
    //       class: 'modal-lg',
    //       initialState: {
    //         id: id,
    //       },
    //     }
    //   );
    // }

    createOrEditPaymentAccountDialog.content.paymentAccountType = paymentAccountType;

    createOrEditPaymentAccountDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }
}
