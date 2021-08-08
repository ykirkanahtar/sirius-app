import { Component, Injector, OnInit, ViewChild } from "@angular/core";
import { finalize } from "rxjs/operators";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { appModuleAnimation } from "@shared/animations/routerTransition";
import {
  PagedListingComponentBase,
  PagedRequestDto,
} from "@shared/paged-listing-component-base";
import {
  InventoryDto,
  InventoryServiceProxy,
  InventoryGetAllDtoPagedResultDto,
  InventoryGetAllDto,
  QuantityType,
} from "@shared/service-proxies/service-proxies";
import { LazyLoadEvent, SelectItem } from "primeng/api";
import { Table } from "primeng/table";
import { Router } from "@angular/router";
import { CreateInventoryDialogComponent } from "./create-inventory/create-inventory-dialog.component";
import { EditInventoryDialogComponent } from "./edit-inventory/edit-inventory-dialog.component";

class PagedInventoryRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: "./inventories.component.html",
  animations: [appModuleAnimation()],
})
export class InventoriesComponent
  extends PagedListingComponentBase<InventoryDto>
  implements OnInit
{

  @ViewChild("dataTable", { static: true }) dataTable: Table;

  sortingColumn: string;
  advancedFiltersVisible = false;

  inventories: InventoryGetAllDto[] = [];
  QuantityType = QuantityType;

  constructor(
    injector: Injector,
    private _inventoryService: InventoryServiceProxy,
    private _router: Router,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.getDataPage(1);
  }

  createInventory(): void {
    this.showCreateOrEditInventoryDialog();
  }

  editInventory(inventory: InventoryDto): void {
    this.showCreateOrEditInventoryDialog(inventory.id);
  }

  clearFilters(): void {
    this.getDataPage(1);
  }

  getData(event?: LazyLoadEvent) {
    this.sortingColumn = this.primengTableHelper.getSorting(this.dataTable);
    this.getDataPage(1);
  }

  protected list(
    request: PagedInventoryRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    this._inventoryService
      .getAllInventories(
        this.sortingColumn,
        request.skipCount,
        request.maxResultCount
      )
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: InventoryGetAllDtoPagedResultDto) => {
        this.inventories = result.items;
        this.showPaging(result, pageNumber);
      });
  }

  public deleteByGroup(inventory: InventoryGetAllDto): void {
    abp.message.confirm(
      this.l("InventoryDeleteWarningMessage"),
      undefined,
      (result: boolean) => {
        if (result) {
          this._inventoryService
            .deleteFromInventoryPage(
              inventory.inventoryTypeId,
              inventory.quantity,
              inventory.serialNumber === null ? "" : inventory.serialNumber
            )
            .subscribe(() => {
              abp.notify.success(this.l("SuccessfullyDeleted"));
              this.refresh();
            });
        }
      }
    );
  }

  protected delete(entity: InventoryDto): void {
    throw new Error("Method not implemented.");
  }

  private showCreateOrEditInventoryDialog(id?: string): void {
    let createOrEditInventoryDialog: BsModalRef;
    if (!id) {
      createOrEditInventoryDialog = this._modalService.show(
        CreateInventoryDialogComponent,
        {
          class: "modal-lg",
        }
      );
    } else {
      createOrEditInventoryDialog = this._modalService.show(
        EditInventoryDialogComponent,
        {
          class: "modal-lg",
          initialState: {
            id: id,
          },
        }
      );
    }

    createOrEditInventoryDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }
}
