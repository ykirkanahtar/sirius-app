import { Component, Injector } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {
  PagedListingComponentBase,
  PagedRequestDto
} from '@shared/paged-listing-component-base';
import { AccountBookDto, AccountBookServiceProxy, AccountBookDtoPagedResultDto  } from '@shared/service-proxies/service-proxies';

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
            .subscribe(() => {});
        }
      }
    );
  }

  createAccountBook(): void {
    this.showCreateOrEditAccountBookDialog();
  }

  showCreateOrEditAccountBookDialog(id?: number): void {
    let createOrEditAccountBookDialog: BsModalRef;
    if (!id) {
      // createOrEditAccountBookDialog = this._modalService.show(
      //   CreateAccountBookDialogComponent,
      //   {
      //     class: 'modal-lg',
      //   }
      // );
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
