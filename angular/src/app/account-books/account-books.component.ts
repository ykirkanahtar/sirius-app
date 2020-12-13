import { Component, HostListener, Injector, OnInit, ViewChild } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {
  PagedListingComponentBase,
  PagedRequestDto,
} from '@shared/paged-listing-component-base';
import {
  AccountBookDto,
  AccountBookServiceProxy,
  HousingServiceProxy,
  PersonServiceProxy,
  LookUpDto,
  PaymentCategoryServiceProxy,
  PaymentAccountServiceProxy,
  AccountBookGetAllOutputPagedResultDto,
  AccountBookGetAllOutput,
} from '@shared/service-proxies/service-proxies';
import { CreateHousingDueAccountBookDialogComponent } from './create-account-book/create-housing-due-account-book-dialog.component';
import { CreateOtherPaymentAccountBookDialogComponent } from './create-account-book/create-other-payment-account-book-dialog.component';
import { Table } from 'primeng/table';
import { LazyLoadEvent, SelectItem } from 'primeng/api';
import * as moment from 'moment';

class PagedAccountBooksRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: './account-books.component.html',
  animations: [appModuleAnimation()],
})
export class AccountBooksComponent
  extends PagedListingComponentBase<AccountBookDto>
  implements OnInit {
  @ViewChild('dataTable', { static: true }) dataTable: Table;

  sortingColumn: string;
  advancedFiltersVisible = false;

  accountBooks: AccountBookGetAllOutput[] = [];
  accountBookFiles: string[] = [];

  paymentCategoriesFilter: SelectItem[] = [];
  selectedPaymentCategoriesFilter: string[] = [];

  housingsFilters: SelectItem[] = [];
  selectedHousingsFilters: string[] = [];

  peopleFilters: SelectItem[] = [];
  selectedPeopleFilters: string[] = [];

  fromPaymentAccountsFilter: SelectItem[] = [];
  selectedFromPaymentAccountsFilter: string[] = [];

  toPaymentAccountsFilter: SelectItem[] = [];
  selectedToPaymentAccountsFilter: string[] = [];

  startDateFilter: moment.Moment;
  endDateFilter: moment.Moment;

  display: boolean = false;
  closeDialog: boolean = false;

  responsiveOptions2:any[] = [
    {
        breakpoint: '1500px',
        numVisible: 5
    },
    {
        breakpoint: '1024px',
        numVisible: 3
    },
    {
        breakpoint: '768px',
        numVisible: 2
    },
    {
        breakpoint: '560px',
        numVisible: 1
    }
 ];

  constructor(
    injector: Injector,
    private _accountBooksService: AccountBookServiceProxy,
    private _paymentCategoryService: PaymentCategoryServiceProxy,
    private _housingService: HousingServiceProxy,
    private _paymentAccountService: PaymentAccountServiceProxy,
    private _personService: PersonServiceProxy,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {

    this._paymentCategoryService
      .getPaymentCategoryLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.paymentCategoriesFilter = result;
      });

    this._housingService.getHousingLookUp().subscribe((result: LookUpDto[]) => {
      this.housingsFilters = result;
    });

    this._personService.getPersonLookUp().subscribe((result: LookUpDto[]) => {
      this.peopleFilters = result;
    });

    this._paymentAccountService
      .getPaymentAccountLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.fromPaymentAccountsFilter = result;
        this.toPaymentAccountsFilter = result;
      });

    this.getDataPage(1);
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

  clearFilters(): void {
    this.selectedPaymentCategoriesFilter = [];
    this.selectedHousingsFilters = [];
    this.selectedPeopleFilters = [];
    this.selectedFromPaymentAccountsFilter = [];
    this.selectedToPaymentAccountsFilter = [];
    this.startDateFilter = null;
    this.endDateFilter = null;
    this.getDataPage(1);
  }

  getData(event?: LazyLoadEvent) {
    this.sortingColumn = this.primengTableHelper.getSorting(this.dataTable);
    this.getDataPage(1);
  }

  showImages(accountBookFiles: string[]) {
    this.display = true;
    this.accountBookFiles = accountBookFiles;
  }

  @HostListener('document:keydown.escape', ['$event']) onKeydownHandler(event: KeyboardEvent) {
    if (this.display) {
      this.display = !this.display;
    }
  }

  protected list(
    request: PagedAccountBooksRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    this._accountBooksService
      .getAllList(
        this.startDateFilter,
        this.endDateFilter,
        this.selectedPaymentCategoriesFilter,
        this.selectedHousingsFilters,
        this.selectedPeopleFilters,
        this.selectedFromPaymentAccountsFilter,
        this.selectedToPaymentAccountsFilter,
        this.sortingColumn,
        request.skipCount,
        request.maxResultCount
      )
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: AccountBookGetAllOutputPagedResultDto) => {
        this.accountBooks = result.items;
        this.showPaging(result, pageNumber);
      });
  }

  protected delete(accountBook: AccountBookDto): void {
    abp.message.confirm(
      this.l('AccountBookDeleteWarningMessage', accountBook.id),
      undefined,
      (result: boolean) => {
        if (result) {
          this._accountBooksService.delete(accountBook.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  private showCreateOrEditAccountBookDialog(
    isHousingDue: boolean,
    id?: number
  ): void {
    let createOrEditAccountBookDialog: BsModalRef;
    if (!id) {
      createOrEditAccountBookDialog = this._modalService.show(
        isHousingDue
          ? CreateHousingDueAccountBookDialogComponent
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
