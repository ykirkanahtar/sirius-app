import { Component, Injector } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {
  PagedListingComponentBase,
  PagedRequestDto
} from '@shared/paged-listing-component-base';
import { CreatePaymentCategoryDialogComponent } from './create-payment-category/create-payment-category-dialog.component';
import { EditPaymentCategoryDialogComponent } from './edit-payment-category/edit-payment-category-dialog.component';
import { PaymentCategoryDto, PaymentCategoryServiceProxy, PaymentCategoryDtoPagedResultDto } from '@shared/service-proxies/service-proxies';

class PagedHousingsRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: './payment-categories.component.html',
  animations: [appModuleAnimation()]
})
export class PaymentCategoriesComponent extends PagedListingComponentBase<PaymentCategoryDto> {
  paymentCategories: PaymentCategoryDto[] = [];
  keyword = '';

  constructor(
    injector: Injector,
    private _paymentCategoryService: PaymentCategoryServiceProxy,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  list(
    request: PagedHousingsRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;

    this._paymentCategoryService
      .getAll(request.keyword, true, request.skipCount, request.maxResultCount)
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: PaymentCategoryDtoPagedResultDto) => {
        this.paymentCategories = result.items;
        this.showPaging(result, pageNumber);
      });
  }

  delete(paymentCategory: PaymentCategoryDto): void {
    abp.message.confirm(
      this.l('PaymentCategoryDeleteWarningMessage', paymentCategory.paymentCategoryName),
      undefined,
      (result: boolean) => {
        if (result) {
          this._paymentCategoryService
            .delete(paymentCategory.id)
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

  createPaymentCategory(): void {
    this.showCreateOrEditPaymentCategoryDialog();
  }

  editPaymentCategory(paymentCategory: PaymentCategoryDto): void {
    this.showCreateOrEditPaymentCategoryDialog(paymentCategory.id);
  }

  showCreateOrEditPaymentCategoryDialog(id?: string): void {
    let createOrEditPaymentCategoryDialog: BsModalRef;
    if (!id) {
      createOrEditPaymentCategoryDialog = this._modalService.show(
        CreatePaymentCategoryDialogComponent,
        {
          class: 'modal-lg',
        }
      );
    } else {
      createOrEditPaymentCategoryDialog = this._modalService.show(
        EditPaymentCategoryDialogComponent,
        {
          class: 'modal-lg',
          initialState: {
            id: id,
          },
        }
      );
    }

    createOrEditPaymentCategoryDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }
}
