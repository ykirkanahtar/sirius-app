import { Component, Injector, OnInit, ViewChild } from '@angular/core';
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
import { Table } from 'primeng/table';
import { LazyLoadEvent } from 'primeng/api';

class PagedHousingsRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: './payment-categories.component.html',
  animations: [appModuleAnimation()]
})
export class PaymentCategoriesComponent extends PagedListingComponentBase<PaymentCategoryDto>
  implements OnInit {

  @ViewChild('dataTable', { static: true }) dataTable: Table;

  sortingColumn: string;
  advancedFiltersVisible = false;

  paymentCategories: PaymentCategoryDto[] = [];

  paymentCategoriesFilter: string[] = [];
  selectedPaymentCategoryFilter: string;

  constructor(
    injector: Injector,
    private _paymentCategoryService: PaymentCategoryServiceProxy,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.getDataPage(1);
  }

  searchPaymentCategory(event) {
    this._paymentCategoryService
      .getPaymentCategoryFromAutoCompleteFilter(event.query)
      .subscribe((result) => {
        this.paymentCategoriesFilter = result;
      });
  }

  createPaymentCategory(): void {
    this.showCreateOrEditPaymentCategoryDialog();
  }

  editPaymentCategory(paymentCategory: PaymentCategoryDto): void {
    this.showCreateOrEditPaymentCategoryDialog(paymentCategory.id);
  }

  clearFilters(): void {
    this.paymentCategoriesFilter = [];
    this.selectedPaymentCategoryFilter = '';
    this.getDataPage(1);
  }

  getData(event?: LazyLoadEvent) {
    this.sortingColumn = this.primengTableHelper.getSorting(this.dataTable);
    this.getDataPage(1);
  }

  protected list(
    request: PagedHousingsRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    this._paymentCategoryService
      .getAll(this.selectedPaymentCategoryFilter, this.sortingColumn, request.skipCount, request.maxResultCount)
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

  protected delete(paymentCategory: PaymentCategoryDto): void {
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
            .subscribe(() => { });
        }
      }
    );
  }

  private showCreateOrEditPaymentCategoryDialog(id?: string): void {
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
