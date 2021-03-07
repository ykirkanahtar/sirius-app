import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {
  PagedListingComponentBase,
  PagedRequestDto,
} from '@shared/paged-listing-component-base';
import { CreatePeriodDialogComponent } from './create-period/create-period-dialog.component';
import { EditPeriodDialogComponent } from './edit-period/edit-period-dialog.component';
import {
  PeriodDto,
  PeriodServiceProxy,
  PeriodDtoPagedResultDto,
  LookUpDto,
  SiteOrBlock,
  BlockServiceProxy,
} from '@shared/service-proxies/service-proxies';
import { LazyLoadEvent, SelectItem } from 'primeng/api';
import { Table } from 'primeng/table';
import { MenuItem as PrimeNgMenuItem } from "primeng/api";
import { Router } from '@angular/router';

class PagedPeriodRequestDto extends PagedRequestDto {
  keyword: string;
}

enum PeriodType {
  Site,
  Block
}

@Component({
  templateUrl: './periods.component.html',
  animations: [appModuleAnimation()],
})
export class PeriodsComponent
  extends PagedListingComponentBase<PeriodDto>
  implements OnInit {
  @ViewChild('dataTable', { static: true }) dataTable: Table;

  sortingColumn: string;
  advancedFiltersVisible = false;

  items: PrimeNgMenuItem[] = [];

  periods: PeriodDto[] = [];

  nameFilter: string;
  siteOrBlockItems = [];
  siteOrBlockFilter: number;
  blockItems = [];
  blockIdFilter: string;
  periodType: PeriodType;
  PeriodTypeEnum = PeriodType;

  constructor(
    injector: Injector,
    private _periodService: PeriodServiceProxy,
    private _blockService: BlockServiceProxy,
    private _router: Router,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {

    var url = this._router.routerState.snapshot.url;
    this.periodType = url.includes('site') ? PeriodType.Site : PeriodType.Block;
 
    this.siteOrBlockItems = Object.keys(SiteOrBlock).map(k => ({name: k, value: SiteOrBlock[k as any]}));
    this.siteOrBlockFilter = SiteOrBlock.Site;
    
    this._blockService
    .getBlockLookUp()
    .subscribe((result: LookUpDto[]) => {
      this.blockItems = result;
    });

    this.getDataPage(1);
  }

  createPeriodForSite(): void {
    this.showCreateOrEditPeriodDialog(SiteOrBlock.Site);
  }

  createPeriodForBlock(): void {
    this.showCreateOrEditPeriodDialog(SiteOrBlock.Block);
  }

  editPeriod(period: PeriodDto): void {
    this.showCreateOrEditPeriodDialog(null, period.id);
  }

  clearFilters(): void {
    this.getDataPage(1);
  }

  getData(event?: LazyLoadEvent) {
    this.sortingColumn = this.primengTableHelper.getSorting(this.dataTable);
    this.getDataPage(1);
  }

  protected list(
    request: PagedPeriodRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    this._periodService
      .getAll(
        this.nameFilter,
        this.siteOrBlockFilter,
        this.blockIdFilter,
        this.sortingColumn,
        request.skipCount,
        request.maxResultCount
      )
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: PeriodDtoPagedResultDto) => {
        this.periods = result.items;
        this.showPaging(result, pageNumber);
      });
  }

  protected delete(period: PeriodDto): void {
    abp.message.confirm(
      this.l(
        'PeriodDeleteWarningMessage',
        period.name
      ),
      undefined,
      (result: boolean) => {
        if (result) {
          this._periodService.delete(period.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  private showCreateOrEditPeriodDialog(siteOrBlock?: SiteOrBlock, id?: string): void {
    let createOrEditPeriodDialog: BsModalRef;
    if (!id) {
      createOrEditPeriodDialog = this._modalService.show(
        CreatePeriodDialogComponent,
        {
          class: 'modal-lg',
          initialState: {
            siteOrBlock: siteOrBlock,
          },
        }
      );
    } else {
      createOrEditPeriodDialog = this._modalService.show(
        EditPeriodDialogComponent,
        {
          class: 'modal-lg',
          initialState: {
            id: id,
          },
        }
      );
    }

    createOrEditPeriodDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }
}
