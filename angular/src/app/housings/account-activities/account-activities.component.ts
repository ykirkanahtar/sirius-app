import {
  Component,
  Injector,
  EventEmitter,
  Output,
  ViewChild,
  OnInit,
} from "@angular/core";
import { BsModalRef } from "ngx-bootstrap/modal";
import * as _ from "lodash";
import {
  BlockDto,
  HousingDto,
  HousingPaymentPlanDto,
  HousingPaymentPlanDtoPagedResultDto,
  HousingPaymentPlanServiceProxy,
  HousingPaymentPlanType,
  HousingServiceProxy,
  CreditOrDebt,
  LookUpDto,
  PaymentCategoryServiceProxy,
  HousingPaymentPlanExportOutput,
  PagedHousingPaymentPlanResultDto,
} from "@shared/service-proxies/service-proxies";
import { PagedRequestDto } from "@shared/paged-listing-component-base";
import { appModuleAnimation } from "@shared/animations/routerTransition";
import { Table } from "primeng/table";
import { LazyLoadEvent, SelectItem } from "primeng/api";
import { AppComponentBase } from "@shared/app-component-base";
import * as moment from "moment";
import { CommonFunctions } from "@shared/helpers/CommonFunctions";
import * as xlsx from "xlsx";

class PagedHousingPaymentPlanResultRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: "./account-activities.component.html",
  animations: [appModuleAnimation()],
})
export class AccountActivitiesDialogComponent
  extends AppComponentBase
  implements OnInit {
  @ViewChild("dataTable", { static: true }) dataTable: Table;

  sortingColumn: string;
  advancedFiltersVisible = false;

  skipCount: number;
  maxResultCount: number;
  totalRecords: number;
  balance: number;
  isTableLoading: boolean = false;

  housingPaymentPlans: HousingPaymentPlanDto[] = [];
  housing = new HousingDto();
  block = new BlockDto();
  housingPaymentPlanTypeEnum = HousingPaymentPlanType;
  creditOrDebtEnum = CreditOrDebt;

  id: string;
  @Output() onSave = new EventEmitter<any>();

  creditOrDebtsFilter: SelectItem[] = [];
  selectedCreditOrDebtFilter: CreditOrDebt[] = [];

  housingPaymentPlanTypeFilter: SelectItem[] = [];
  selectedHousingPaymentPlanTypesFilter: HousingPaymentPlanType[] = [];

  paymentCategoriesFilter: SelectItem[] = [];
  selectedPaymentCategoriesFilter: string[] = [];

  startDateFilter: moment.Moment;
  endDateFilter: moment.Moment;

  data: any;
  barOptions: any;

  constructor(
    injector: Injector,
    private _housingService: HousingServiceProxy,
    private _housingPaymentPlanService: HousingPaymentPlanServiceProxy,
    private _paymentCategoryService: PaymentCategoryServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {

    this.barOptions = {
      responsive: true,
      tooltips: {
        mode: 'index',
        intersect: true,
      },
      scales: {
        yAxes: [
          {
            type: 'linear',
            display: true,
            position: 'left',
            id: 'y-axis-1',
            ticks: { min: 0 },
          },
        ],
      },
    };

    this._housingService.get(this.id).subscribe((result: HousingDto) => {
      this.housing = result;
      this.block = this.housing.block;
    });

    //enum işlemleri
    let housingPaymentPlanTypeMap = this.enumToMap(HousingPaymentPlanType);
    let housingPaymentPlanTypes = Array.from(housingPaymentPlanTypeMap.keys());

    for (let i = 0; i < housingPaymentPlanTypes.length; i++) {
      this.housingPaymentPlanTypeFilter.push({
        label: this.l(housingPaymentPlanTypes[i]),
        value: this.housingPaymentPlanTypeEnum[housingPaymentPlanTypes[i]],
      });
    }

    let creditOrDebtMap = this.enumToMap(CreditOrDebt);
    let creditOrDebts = Array.from(creditOrDebtMap.keys());

    for (let i = 0; i < creditOrDebts.length; i++) {
      this.creditOrDebtsFilter.push({
        label: this.l(creditOrDebts[i]),
        value: this.creditOrDebtEnum[creditOrDebts[i]],
      });
    }
    //enum işlemleri

    this._paymentCategoryService
      .getHousingDueLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.paymentCategoriesFilter = result;
      });
  }

  clearFilters(): void {
    this.selectedCreditOrDebtFilter = [];
    this.selectedPaymentCategoriesFilter = [];
    this.selectedHousingPaymentPlanTypesFilter = [];
    this.startDateFilter = null;
    this.endDateFilter = null;
    this.getData();
  }

  enumToMap(enumeration: any): Map<string, string | number> {
    const map = new Map<string, string | number>();
    for (let key in enumeration) {
      //TypeScript does not allow enum keys to be numeric
      if (!isNaN(Number(key))) continue;

      const val = enumeration[key] as string | number;

      //TypeScript does not allow enum value to be null or undefined
      if (val !== undefined && val !== null) map.set(key, val);
    }

    return map;
  }

  getData(event?: LazyLoadEvent) {
    this.isTableLoading = true;

    this.sortingColumn = this.primengTableHelper.getSorting(this.dataTable);

    if (event === undefined) {
      this.skipCount = 0;
      this.maxResultCount = 10;
    } else {
      if (event.first || event.first === 0) {
        this.skipCount = event.first;
      }

      if (event.first || event.rows) {
        this.maxResultCount = event.first + event.rows;
      }
    }

    this._housingPaymentPlanService
      .getAllByHousingId(
        this.id,
        undefined,
        this.startDateFilter,
        this.endDateFilter,
        this.selectedPaymentCategoriesFilter,
        this.selectedCreditOrDebtFilter,
        this.selectedHousingPaymentPlanTypesFilter,
        this.sortingColumn,
        this.skipCount,
        this.maxResultCount
      )
      .subscribe((result: PagedHousingPaymentPlanResultDto) => {
        this.housingPaymentPlans = result.items;

        this.data = {
          labels: ['Aidatlar'],
          datasets: [
              {
                label: 'Aidat tanımı',
                backgroundColor: '#9CCC65',
                borderColor: '#7CB342',
                data: [result.debtBalance]
              },
              {
                  label: 'Aidat ödemesi',
                  backgroundColor: '#42A5F5',
                  borderColor: '#1E88E5',
                  data: [result.creditBalance]
              }
          ]
        };

        this.balance = result.balance;
        this.totalRecords = result.totalCount;
        this.isTableLoading = false;
      });
  }

  getAmount(amount: number, creditOrDebt: CreditOrDebt): number {
    return creditOrDebt == CreditOrDebt.Credit ? amount * -1 : amount;
  }

  exportExcel() {
    this._housingPaymentPlanService
      .getAllByHousingIdForExport(
        this.id,
        undefined,
        this.startDateFilter,
        this.endDateFilter,
        this.selectedPaymentCategoriesFilter,
        this.selectedCreditOrDebtFilter,
        this.selectedHousingPaymentPlanTypesFilter,
        this.sortingColumn
      )
      .subscribe((result: HousingPaymentPlanExportOutput[]) => {
        CommonFunctions.createExcelFile(result, xlsx, this, this.l("HousingPaymentPlan"));
      });
  }
}

