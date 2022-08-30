import { Component, Injector, OnInit, ViewChild } from "@angular/core";
import { finalize } from "rxjs/operators";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { appModuleAnimation } from "@shared/animations/routerTransition";
import {
  PagedListingComponentBase,
  PagedRequestDto,
} from "@shared/paged-listing-component-base";
import { CreatePaymentCategoryDialogComponent } from "./create-payment-category/create-payment-category-dialog.component";
import { EditPaymentCategoryDialogComponent } from "./edit-payment-category/edit-payment-category-dialog.component";
import {
  PaymentCategoryDto,
  PaymentCategoryServiceProxy,
  PaymentCategoryDtoPagedResultDto,
  PaymentCategoryType,
} from "@shared/service-proxies/service-proxies";
import { Table } from "primeng/table";
import { LazyLoadEvent } from "primeng/api";
import { MenuItem as PrimeNgMenuItem } from "primeng/api";

class PagedHousingsRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: "./payment-categories.component.html",
  animations: [appModuleAnimation()],
})
export class PaymentCategoriesComponent
  extends PagedListingComponentBase<PaymentCategoryDto>
  implements OnInit {
  @ViewChild("dataTable", { static: true }) dataTable: Table;

  items: PrimeNgMenuItem[];

  sortingColumn: string;
  advancedFiltersVisible = false;

  paymentCategories: PaymentCategoryDto[] = [];

  paymentCategoriesFilter: string[] = [];
  selectedPaymentCategoryFilter: string;
  isPassive: boolean;

  constructor(
    injector: Injector,
    private _paymentCategoryService: PaymentCategoryServiceProxy,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.items = [
      {
        label: this.l("ExpensePaymentCategory"),
        icon: "pi pi-arrow-right",
        command: () => {
          this.createPaymentCategory(PaymentCategoryType.Expense, false);
        },
      },
      {
        label: this.l("IncomePaymentCategory"),
        icon: "pi pi-arrow-left",
        command: () => {
          this.createPaymentCategory(PaymentCategoryType.Income, false);
        },
      },
      {
        label: this.l("TransferBetweenAccounts"),
        icon: "pi pi-sort-alt",
        command: () => {
          this.createPaymentCategory(
            PaymentCategoryType.TransferBetweenAccounts, false
          );
        },
      },
    ];

    this.getDataPage(1);
  }

  searchPaymentCategory(event) {
    this._paymentCategoryService
      .getPaymentCategoryFromAutoCompleteFilter(event.query)
      .subscribe((result) => {
        this.paymentCategoriesFilter = result;
      });
  }

  createPaymentCategory(paymentCategoryType: PaymentCategoryType, housingDue: boolean): void {
    this.showCreateOrEditPaymentCategoryDialog(paymentCategoryType, housingDue);
  }

  editPaymentCategory(paymentCategory: PaymentCategoryDto): void {
    this.showCreateOrEditPaymentCategoryDialog(
      paymentCategory.paymentCategoryType,
      null,
      paymentCategory.id
    );
  }

  clearFilters(): void {
    this.paymentCategoriesFilter = [];
    this.selectedPaymentCategoryFilter = "";
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
      .getAll(
        this.selectedPaymentCategoryFilter,
        this.isPassive,
        this.sortingColumn,
        request.skipCount,
        request.maxResultCount
      )
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
      this.l(
        "PaymentCategoryDeleteWarningMessage",
        paymentCategory.paymentCategoryName
      ),
      undefined,
      (result: boolean) => {
        if (result) {
          this._paymentCategoryService
            .delete(paymentCategory.id)
            .subscribe(() => {
              abp.notify.success(this.l("SuccessfullyDeleted"));
              this.refresh();
            });
        }
      }
    );
  }

  private showCreateOrEditPaymentCategoryDialog(
    paymentCategoryType?: PaymentCategoryType,
    housingDue?: boolean,
    id?: string
  ): void {
    let createOrEditPaymentCategoryDialog: BsModalRef;
    if (!id) {
      createOrEditPaymentCategoryDialog = this._modalService.show(
        CreatePaymentCategoryDialogComponent,
        {
          class: "modal-lg",
          initialState: {
            paymentCategoryType: paymentCategoryType,
            housingDue: housingDue,
          },
        }
      );
    } else {
      createOrEditPaymentCategoryDialog = this._modalService.show(
        EditPaymentCategoryDialogComponent,
        {
          class: "modal-lg",
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
