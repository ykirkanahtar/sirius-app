import {
  Component,
  Injector,
  EventEmitter,
  Output,
  ViewChild,
  OnInit,
} from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import * as _ from 'lodash';
import {
  BlockDto,
  HousingDto,
  HousingPaymentPlanDto,
  HousingPaymentPlanDtoPagedResultDto,
  HousingPaymentPlanServiceProxy,
  HousingPaymentPlanType,
  HousingServiceProxy,
  CreditOrDebt
} from '@shared/service-proxies/service-proxies';
import {
  PagedRequestDto
} from '@shared/paged-listing-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/table';
import { LazyLoadEvent } from 'primeng/api';
import { AppComponentBase } from '@shared/app-component-base';

class PagedHousingPaymentPlanResultRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: './account-activities.component.html',
  animations: [appModuleAnimation()]
})
export class AccountActivitiesDialogComponent extends AppComponentBase
  implements OnInit {

  @ViewChild('dataTable', { static: true }) dataTable: Table;

  sortingColumn: string;
  skipCount: number;
  maxResultCount: number;
  totalRecords: number;
  isTableLoading: boolean = false;

  housingPaymentPlans: HousingPaymentPlanDto[] = [];
  housing = new HousingDto();
  block = new BlockDto();
  creditOrDebtEnum = CreditOrDebt;
  housingPaymentPlanTypeEnum = HousingPaymentPlanType;

  id: string;
  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _housingService: HousingServiceProxy,
    private _housingPaymentPlanService: HousingPaymentPlanServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {

    this._housingService
      .get(this.id)
      .subscribe((result: HousingDto) => {
        this.housing = result;
        this.block = this.housing.block;
      });
  }

  getData(event?: LazyLoadEvent) {
    this.isTableLoading = true;

    this.sortingColumn = this.primengTableHelper.getSorting(this.dataTable);
    
    if (event.first || event.first === 0) {
      this.skipCount = event.first;
    }

    if (event.first || event.rows) {
      this.maxResultCount = event.first + event.rows;
    }

    this._housingPaymentPlanService
      .getAllByHousingId(this.id, this.sortingColumn, this.skipCount, this.maxResultCount)
      .subscribe((result: HousingPaymentPlanDtoPagedResultDto) => {
        this.housingPaymentPlans = result.items;
        this.totalRecords = result.totalCount;
        this.isTableLoading = false;
      });
  }
}
