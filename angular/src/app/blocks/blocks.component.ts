import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {
  PagedListingComponentBase,
  PagedRequestDto
} from '@shared/paged-listing-component-base';
import { BlockDto, BlockServiceProxy, BlockDtoPagedResultDto } from '@shared/service-proxies/service-proxies';
import { Table } from 'primeng/table';
import { LazyLoadEvent } from 'primeng/api';
import { EditBlockDialogComponent } from './edit-block/edit-block-dialog.component';
import { CreateBlockDialogComponent } from './create-block/create-block-dialog.component';

class PagedEmployeesRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: './blocks.component.html',
  animations: [appModuleAnimation()]
})
export class BlocksComponent extends PagedListingComponentBase<BlockDto>
  implements OnInit {

  @ViewChild('dataTable', { static: true }) dataTable: Table;

  sortingColumn: string;

  blocks: BlockDto[] = [];

  constructor(
    injector: Injector,
    private _blockService: BlockServiceProxy,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.getDataPage(1);
  }

  createBlock(): void {
    this.showCreateOrEditBlockDialog();
  }

  editBlock(block: BlockDto): void {
    this.showCreateOrEditBlockDialog(block.id);
  }

  clearFilters(): void {
    this.getDataPage(1);
  }

  getData(event?: LazyLoadEvent) {
    this.sortingColumn = this.primengTableHelper.getSorting(this.dataTable);
    this.getDataPage(1);
  }

  protected list(
    request: PagedEmployeesRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    this._blockService
      .getAll(
        this.sortingColumn,
        request.skipCount,
        request.maxResultCount)
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: BlockDtoPagedResultDto) => {
        this.blocks = result.items;
        this.showPaging(result, pageNumber);
      });
  }

  protected delete(block: BlockDto): void {
    abp.message.confirm(
      this.l('BlockDeleteWarningMessage', block.blockName),
      undefined,
      (result: boolean) => {
        if (result) {
          this._blockService
            .delete(block.id)
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

  private showCreateOrEditBlockDialog(id?: string): void {
    let createOrEditBlockDialog: BsModalRef;
    if (!id) {
      createOrEditBlockDialog = this._modalService.show(
        CreateBlockDialogComponent,
        {
          class: 'modal-lg',
        }
      );
    } else {
      createOrEditBlockDialog = this._modalService.show(
        EditBlockDialogComponent,
        {
          class: 'modal-lg',
          initialState: {
            id: id,
          },
        }
      );
    }

    createOrEditBlockDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }
}
