import { Component, Injector } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {
  PagedListingComponentBase,
  PagedRequestDto
} from '@shared/paged-listing-component-base';
import { CreatePaymentAccountDialogComponent } from './create-payment-account/create-payment-account-dialog.component';
import { PaymentAccountDto, PaymentAccountServiceProxy, PaymentAccountDtoPagedResultDto, PaymentAccountType } from '@shared/service-proxies/service-proxies';

class PagedPaymentAccountRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: './payment-accounts.component.html',
  animations: [appModuleAnimation()]
})
export class PaymentAccountsComponent extends PagedListingComponentBase<PaymentAccountDto> {
  paymentAccounts: PaymentAccountDto[] = [];
  keyword = '';
  paymentAccountTypeEnum = PaymentAccountType;

  constructor(
    injector: Injector,
    private _paymentAccountService: PaymentAccountServiceProxy,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  list(
    request: PagedPaymentAccountRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;

    this._paymentAccountService
      .getAll(request.keyword, true, request.skipCount, request.maxResultCount)
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

  delete(paymentAccount: PaymentAccountDto): void {
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
            .subscribe(() => {});
        }
      }
    );
  }

  createPaymentAccount(paymentAccountType: PaymentAccountType): void {
    this.showCreateOrEditPaymentAccountDialog(paymentAccountType);
  }

  showCreateOrEditPaymentAccountDialog(paymentAccountType: PaymentAccountType, id?: number): void {
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