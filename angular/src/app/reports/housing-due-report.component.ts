import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {
  HousingDueReportDto,
  ReportsServiceProxy,
} from '@shared/service-proxies/service-proxies';
import { LazyLoadEvent } from 'primeng/api';
import { Table } from 'primeng/table';
import { MenuItem as PrimeNgMenuItem } from "primeng/api";
import { AppComponentBase } from '@shared/app-component-base';

@Component({
  templateUrl: './housing-due-report.component.html',
  animations: [appModuleAnimation()],
})
export class HousingDueReportComponent extends AppComponentBase
  implements OnInit {

  @ViewChild('dataTable', { static: true }) dataTable: Table;

  sortingColumn: string;
  advancedFiltersVisible = false;

  items: PrimeNgMenuItem[] = [];
  housingDues: HousingDueReportDto[] = [];

  periodItems: any;
  periodIdFilter: string;
  reportFilter: string;
  totalItems: number;

  isTableLoading: boolean;

  constructor(
    injector: Injector,
    private _reportsService: ReportsServiceProxy
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.list();
  }

  clearFilters(): void {
    
  }

  getData(event?: LazyLoadEvent) {
    this.list();
  }

  refresh(): void {
    this.getData();
}

  public list(): void {
    this.isTableLoading = true;
    this._reportsService
      .getHousingDueReport(
        this.reportFilter
      )
      .pipe(
        finalize(() => {

        })
      )
      .subscribe((result: HousingDueReportDto[]) => {
        this.housingDues = result;
        this.isTableLoading = false;
      });
  }

  protected delete(entity: HousingDueReportDto): void {
    throw new Error('Method not implemented.');
  }
}
