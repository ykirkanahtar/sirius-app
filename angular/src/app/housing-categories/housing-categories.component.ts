import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {
  PagedListingComponentBase,
  PagedRequestDto,
} from '@shared/paged-listing-component-base';
import { CreateHousingCategoryDialogComponent } from './create-housing-category/create-housing-category-dialog.component';
import { EditHousingCategoryDialogComponent } from './edit-housing-category/edit-housing-category-dialog.component';
import {
  HousingCategoryDto,
  HousingCategoryServiceProxy,
  HousingCategoryDtoPagedResultDto,
} from '@shared/service-proxies/service-proxies';
import { Table } from 'primeng/table';
import { LazyLoadEvent } from 'primeng/api';

class PagedHousingsRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: './housing-categories.component.html',
  animations: [appModuleAnimation()],
})
export class HousingCategoriesComponent
  extends PagedListingComponentBase<HousingCategoryDto>
  implements OnInit {
  @ViewChild('dataTable', { static: true }) dataTable: Table;

  sortingColumn: string;
  advancedFiltersVisible = false;

  housingCategories: HousingCategoryDto[] = [];

  housingCategoriesFilter: string[] = [];
  selectedHousingCategoryFilter: string;

  constructor(
    injector: Injector,
    private _housingCategoryService: HousingCategoryServiceProxy,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.getDataPage(1);
  }

  createHousingCategory(): void {
    this.showCreateOrEditHousingCategoryDialog();
  }

  editHousingCategory(housingCategory: HousingCategoryDto): void {
    this.showCreateOrEditHousingCategoryDialog(housingCategory.id);
  }

  searchHousingCategory(event) {
    this._housingCategoryService
      .getHousingCategoryFromAutoCompleteFilter(event.query)
      .subscribe((result) => {
        this.housingCategoriesFilter = result;
      });
  }

  clearFilters(): void {
    this.housingCategoriesFilter = [];
    this.selectedHousingCategoryFilter = '';
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
    this._housingCategoryService
      .getAll(
        this.selectedHousingCategoryFilter,
        this.sortingColumn,
        request.skipCount,
        request.maxResultCount
      )
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: HousingCategoryDtoPagedResultDto) => {
        this.housingCategories = result.items;
        this.showPaging(result, pageNumber);
      });
  }

  protected delete(housingCategory: HousingCategoryDto): void {
    abp.message.confirm(
      this.l(
        'HousingCategoryDeleteWarningMessage',
        housingCategory.housingCategoryName
      ),
      undefined,
      (result: boolean) => {
        if (result) {
          this._housingCategoryService
            .delete(housingCategory.id)
            .subscribe(() => {
              abp.notify.success(this.l('SuccessfullyDeleted'));
              this.refresh();
            });
        }
      }
    );
  }

  private showCreateOrEditHousingCategoryDialog(id?: string): void {
    let createOrEditHousingCategoryDialog: BsModalRef;
    if (!id) {
      createOrEditHousingCategoryDialog = this._modalService.show(
        CreateHousingCategoryDialogComponent,
        {
          class: 'modal-lg',
        }
      );
    } else {
      createOrEditHousingCategoryDialog = this._modalService.show(
        EditHousingCategoryDialogComponent,
        {
          class: 'modal-lg',
          initialState: {
            id: id,
          },
        }
      );
    }

    createOrEditHousingCategoryDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }
}
