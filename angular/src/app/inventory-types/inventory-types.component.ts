
import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {
  PagedListingComponentBase,
  PagedRequestDto,
} from '@shared/paged-listing-component-base';
// import { CreatePeriodDialogComponent } from './create-period/create-period-dialog.component';
// import { EditPeriodDialogComponent } from './edit-period/edit-period-dialog.component';
import {
  InventoryTypeDto,
  QuantityType,
  InventoryTypeServiceProxy,
  InventoryTypeDtoPagedResultDto,
} from '@shared/service-proxies/service-proxies';
import { LazyLoadEvent, SelectItem } from 'primeng/api';
import { Table } from 'primeng/table';
import { Router } from '@angular/router';
import { CreateInventoryTypeDialogComponent } from './create-inventory-type/create-inventory-type-dialog.component';
import { EditInventoryTypeDialogComponent } from './edit-inventory-type/edit-inventory-type-dialog.component';

class PagedInventoryTypeRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: './inventory-types.component.html',
  animations: [appModuleAnimation()],
})
export class InventoryTypesComponent
  extends PagedListingComponentBase<InventoryTypeDto>
  implements OnInit {
  @ViewChild('dataTable', { static: true }) dataTable: Table;

  sortingColumn: string;
  advancedFiltersVisible = false;

  inventoryTypes: InventoryTypeDto[] = [];

  quantityType: QuantityType;
  QuantityTypeEnum = QuantityType;

  constructor(
    injector: Injector,
    private _inventoryTypeService: InventoryTypeServiceProxy,
    private _router: Router,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.getDataPage(1);
  }

  createInventoryType(): void {
    this.showCreateOrEditInventoryTypeDialog();
  }

  editInventoryType(inventoryType: InventoryTypeDto): void {
    this.showCreateOrEditInventoryTypeDialog(inventoryType.id);
  }

  clearFilters(): void {
    this.getDataPage(1);
  }

  getData(event?: LazyLoadEvent) {
    this.sortingColumn = this.primengTableHelper.getSorting(this.dataTable);
    this.getDataPage(1);
  }

  protected list(
    request: PagedInventoryTypeRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    this._inventoryTypeService
      .getAll(
        this.sortingColumn,
        request.skipCount,
        request.maxResultCount
      )
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: InventoryTypeDtoPagedResultDto) => {
        this.inventoryTypes = result.items;
        this.showPaging(result, pageNumber);
      });
  }

  protected delete(inventoryType: InventoryTypeDto): void {
    abp.message.confirm(
      this.l(
        'InventoryTypeDeleteWarningMessage',
        inventoryType.inventoryTypeName
      ),
      undefined,
      (result: boolean) => {
        if (result) {
          this._inventoryTypeService.delete(inventoryType.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  private showCreateOrEditInventoryTypeDialog(id?: string): void {
    let createOrEditInventoryTypeDialog: BsModalRef;
    if (!id) {
        createOrEditInventoryTypeDialog = this._modalService.show(
        CreateInventoryTypeDialogComponent,
        {
          class: 'modal-lg',
        }
      );
    } else {
        createOrEditInventoryTypeDialog = this._modalService.show(
        EditInventoryTypeDialogComponent,
        {
          class: 'modal-lg',
          initialState: {
            id: id,
          },
        }
      );
    }

    createOrEditInventoryTypeDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }
}
