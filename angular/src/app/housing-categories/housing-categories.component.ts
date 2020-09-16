import { Component, Injector } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {
  PagedListingComponentBase,
  PagedRequestDto
} from '@shared/paged-listing-component-base';
import { CreateHousingCategoryDialogComponent } from './create-housing-category/create-housing-category-dialog.component';
import { EditHousingCategoryDialogComponent } from './edit-housing-category/edit-housing-category-dialog.component';
import { HousingCategoryDto, HousingCategoryServiceProxy, HousingCategoryDtoPagedResultDto } from '@shared/service-proxies/service-proxies';

class PagedHousingsRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: './housing-categories.component.html',
  animations: [appModuleAnimation()]
})
export class HousingCategoriesComponent extends PagedListingComponentBase<HousingCategoryDto> {
  housingCategories: HousingCategoryDto[] = [];
  keyword = '';

  constructor(
    injector: Injector,
    private _housingCategoryService: HousingCategoryServiceProxy,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  list(
    request: PagedHousingsRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;

    this._housingCategoryService
      .getAll(request.keyword, true, request.skipCount, request.maxResultCount)
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

  delete(housingCategory: HousingCategoryDto): void {
    abp.message.confirm(
      this.l('HousingCategoryDeleteWarningMessage', housingCategory.housingCategoryName),
      undefined,
      (result: boolean) => {
        if (result) {
          this._housingCategoryService
            .delete(housingCategory.id)
            .pipe(
              finalize(() => {
                abp.notify.success(this.l('SuccessfullyDeleted'));
                this.refresh();
              })
            )
            .subscribe(() => {});
        }
      }
    );
  }

  createHousingCategory(): void {
    this.showCreateOrEditHousingCategoryDialog();
  }

  editHousingCategory(housingCategory: HousingCategoryDto): void {
    this.showCreateOrEditHousingCategoryDialog(housingCategory.id);
  }

  showCreateOrEditHousingCategoryDialog(id?: string): void {
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
