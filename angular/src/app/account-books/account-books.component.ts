import { Component, Injector } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {
  PagedListingComponentBase,
  PagedRequestDto
} from '@shared/paged-listing-component-base';
import { AccountBookDto, AccountBookServiceProxy, AccountBookDtoPagedResultDto, HousingDto } from '@shared/service-proxies/service-proxies';
import { CreateHousingDueAccountBookDialogComponent } from './create-account-book/create-housing-due-account-book-dialog.component';
import { CreateOtherPaymentAccountBookDialogComponent } from './create-account-book/create-other-payment-account-book-dialog.component';

class PagedAccountBooksRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: './account-books.component.html',
  animations: [appModuleAnimation()]
})
export class AccountBooksComponent extends PagedListingComponentBase<AccountBookDto> {
  accountBooks: AccountBookDto[] = [];
  keyword = '';

  constructor(
    injector: Injector,
    private _accountBooksService: AccountBookServiceProxy,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  list(
    request: PagedAccountBooksRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;

    this._accountBooksService
      .getAll(request.keyword, true, request.skipCount, request.maxResultCount)
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: AccountBookDtoPagedResultDto) => {
        this.accountBooks = result.items;
        this.showPaging(result, pageNumber);
      });
  }

  getHousingName(housing: HousingDto): string {
    if (housing.block && housing.apartment) {
      return housing.block + ' - ' + housing.apartment;
    }

    if (!housing.block) {
      return housing.apartment;
    }

    if (!housing.apartment) {
      return housing.block;
    }

    return null;
  }

  delete(accountBook: AccountBookDto): void {
    abp.message.confirm(
      this.l('AccountBookDeleteWarningMessage', accountBook.id),
      undefined,
      (result: boolean) => {
        if (result) {
          this._accountBooksService
            .delete(accountBook.id)
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

  createAccountBook(): void {
    this.showCreateOrEditAccountBookDialog(false);
  }

  createHousingDueAccountBook(): void {
    this.showCreateOrEditAccountBookDialog(true);
  }

  createOtherPaymentAccountBook(): void {
    this.showCreateOrEditAccountBookDialog(false);
  }

  showCreateOrEditAccountBookDialog(isHousingDue: boolean, id?: number): void {
    let createOrEditAccountBookDialog: BsModalRef;
    if (!id) {
      createOrEditAccountBookDialog = this._modalService.show(
        isHousingDue ?
          CreateHousingDueAccountBookDialogComponent
          : CreateOtherPaymentAccountBookDialogComponent,
        {
          class: 'modal-lg',
        }
      );
    }
    //else {
    //   createOrEditAccountBookDialog = this._modalService.show(
    //     EditAccountBookDialogComponent,
    //     {
    //       class: 'modal-lg',
    //       initialState: {
    //         id: id,
    //       },
    //     }
    //   );
    // }

    createOrEditAccountBookDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }
}
