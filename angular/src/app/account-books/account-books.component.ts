import {
  Component,
  HostListener,
  Injector,
  OnInit,
  ViewChild,
} from "@angular/core";
import { finalize } from "rxjs/operators";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { appModuleAnimation } from "@shared/animations/routerTransition";
import {
  PagedListingComponentBase,
  PagedRequestDto,
} from "@shared/paged-listing-component-base";
import {
  AccountBookDto,
  AccountBookServiceProxy,
  HousingServiceProxy,
  PersonServiceProxy,
  LookUpDto,
  PaymentCategoryServiceProxy,
  PaymentAccountServiceProxy,
  AccountBookGetAllOutput,
  PagedAccountBookResultDto,
  PaymentCategoryDto,
  PaymentCategoryType,
} from "@shared/service-proxies/service-proxies";
import { CreateAccountBookDialogComponent } from "./create-account-book/create-account-book-dialog.component";
import { Table } from "primeng/table";
import { LazyLoadEvent, SelectItem } from "primeng/api";
import * as moment from "moment";
import { EditAccountBookDialogComponent } from "./edit-account-book/edit-account-book-dialog.component";
import { MenuItem } from "primeng/api";

class PagedAccountBooksRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: "./account-books.component.html",
  animations: [appModuleAnimation()],
})
export class AccountBooksComponent
  extends PagedListingComponentBase<AccountBookDto>
  implements OnInit {
  @ViewChild("dataTable", { static: true }) dataTable: Table;

  sortingColumn: string;
  advancedFiltersVisible = false;

  menuItems: MenuItem[] = [];

  accountBooks: AccountBookGetAllOutput[] = [];
  accountBookFiles: string[] = [];
  lastAccountBookProcessDate: moment.Moment;

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

  responsiveOptions2: any[] = [
    {
      breakpoint: "1500px",
      numVisible: 5,
    },
    {
      breakpoint: "1024px",
      numVisible: 3,
    },
    {
      breakpoint: "768px",
      numVisible: 2,
    },
    {
      breakpoint: "560px",
      numVisible: 1,
    },
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
    this.createMenu();

    this._paymentCategoryService
      .getLookUp(false)
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

  createMenu(): void {
    this._paymentCategoryService
      .getPaymentCategoryForMenu()
      .subscribe((paymentCategories: PaymentCategoryDto[]) => {
        this.menuItems = [];

        if (paymentCategories.length > 15) {
          let incomeMenuItem: MenuItem;
          incomeMenuItem = {
            label: this.l("Income"),
            icon: "pi pi-arrow-left",
            command: () => {
              this.showCreateAccountBookDialogForPaymentCategoryType(
                PaymentCategoryType.Income
              );
            },
          };
          this.menuItems.push(incomeMenuItem);

          let expenseMenuItem: MenuItem;
          expenseMenuItem = {
            label: this.l("Expense"),
            icon: "pi pi-arrow-right",
            command: () => {
              this.showCreateAccountBookDialogForPaymentCategoryType(
                PaymentCategoryType.Expense
              );
            },
          };
          this.menuItems.push(expenseMenuItem);

          let transferBetweenAccounts: MenuItem;
          transferBetweenAccounts = {
            label: this.l("TransferBetweenAccounts"),
            icon: "pi pi-sort-alt",
            command: () => {
              this.showCreateAccountBookDialogForPaymentCategoryType(
                PaymentCategoryType.TransferBetweenAccounts
              );
            },
          };
          this.menuItems.push(transferBetweenAccounts);
        } else {
          let housingDuePaymentCategories = paymentCategories.filter(
            (p) => p.isHousingDue
          );

          if (housingDuePaymentCategories.length > 0) {
            this.createMenuItem(
              housingDuePaymentCategories,
              this.l("HousingDue")
            );
          }

          let incomePaymentCategories = paymentCategories.filter(
            (p) =>
              p.paymentCategoryType === PaymentCategoryType.Income &&
              p.isHousingDue === false
          );

          if (incomePaymentCategories.length > 0) {
            this.createMenuItem(incomePaymentCategories, this.l("Incomes"));
          }

          let expensePaymentCategories = paymentCategories.filter(
            (p) =>
              p.paymentCategoryType === PaymentCategoryType.Expense &&
              p.isHousingDue === false
          );

          if (expensePaymentCategories.length > 0) {
            this.createMenuItem(expensePaymentCategories, this.l("Expenses"));
          }
        }
      });
  }

  createMenuItem(
    groupedPaymentCategories: PaymentCategoryDto[],
    label: string
  ) {
    let groupedMenuItem: MenuItem;

    let groupedMenuItems: MenuItem[] = [];
    for (let index = 0; index < groupedPaymentCategories.length; index++) {
      groupedMenuItem = {
        label: this.l(groupedPaymentCategories[index].paymentCategoryName),
        icon: this.getMenuIcon(
          groupedPaymentCategories[index].paymentCategoryType,
          groupedPaymentCategories[index].isHousingDue
        ),
        command: () => {
          this.showCreateAccountBookDialogForPaymentCategoryType(
            groupedPaymentCategories[index].paymentCategoryType,
            groupedPaymentCategories[index]
          );
        },
      };
      groupedMenuItems.push(groupedMenuItem);
    }

    groupedMenuItem = {
      label: label,
      items: groupedMenuItems,
    };
    this.menuItems.push(groupedMenuItem);
  }

  getMenuIcon(
    paymentCategoryType: PaymentCategoryType,
    isHousingDue: boolean
  ): string {
    if (isHousingDue) {
      return "pi pi-home";
    }

    if (paymentCategoryType === PaymentCategoryType.Income) {
      return "pi pi-arrow-left";
    }

    if (paymentCategoryType === PaymentCategoryType.Expense) {
      return "pi pi-arrow-right";
    }

    if (paymentCategoryType === PaymentCategoryType.TransferBetweenAccounts) {
      return "pi pi-sort-alt";
    }
  }

  editAccountBook(accountBook: AccountBookDto): void {
    this.showEditAccountBookDialog(accountBook.id);
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

  @HostListener("document:keydown.escape", ["$event"]) onKeydownHandler(
    event: KeyboardEvent
  ) {
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
      .subscribe((result: PagedAccountBookResultDto) => {
        this.accountBooks = result.items;
        this.lastAccountBookProcessDate = result.lastAccountBookDate;
        this.showPaging(result, pageNumber);
      });
  }

  protected delete(accountBook: AccountBookDto): void {
    abp.message.confirm(
      this.l("AccountBookDeleteWarningMessage", accountBook.id),
      undefined,
      (result: boolean) => {
        if (result) {
          this._accountBooksService.delete(accountBook.id).subscribe(() => {
            abp.notify.success(this.l("SuccessfullyDeleted"));
            this.refresh();
          });
        }
      }
    );
  }

  private showCreateAccountBookDialogForPaymentCategoryType(
    paymentCategoryType: PaymentCategoryType,
    paymentCategory?: PaymentCategoryDto
  ): void {
    let createAccountBookDialog: BsModalRef;

    createAccountBookDialog = this._modalService.show(
      CreateAccountBookDialogComponent,
      {
        class: "modal-lg",
        initialState: {
          lastAccountBookDate: this.lastAccountBookProcessDate,
          paymentCategoryType: paymentCategoryType,
          paymentCategory: paymentCategory,
        },
      }
    );

    createAccountBookDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }

  private showEditAccountBookDialog(id: string): void {
    let editAccountBookDialog: BsModalRef;

    editAccountBookDialog = this._modalService.show(
      EditAccountBookDialogComponent,
      {
        class: "modal-lg",
        initialState: {
          id: id,
        },
      }
    );

    editAccountBookDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }
}
