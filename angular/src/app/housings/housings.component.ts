import { Component, Injector } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {
  PagedListingComponentBase,
  PagedRequestDto
} from '@shared/paged-listing-component-base';
import { CreateHousingDialogComponent } from './create-housing/create-housing-dialog.component';
import { EditHousingDialogComponent } from './edit-housing/edit-housing-dialog.component';
import { HousingDto, HousingServiceProxy, HousingDtoPagedResultDto } from '@shared/service-proxies/service-proxies';
import { AddPersonDialogComponent } from './add-or-edit-person/add-person-dialog.component';
import { AccountActivitiesDialogComponent } from './account-activities/account-activities.component';

class PagedHousingsRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: './housings.component.html',
  animations: [appModuleAnimation()]
})
export class HousingsComponent extends PagedListingComponentBase<HousingDto> {
  housings: HousingDto[] = [];
  keyword = '';

  constructor(
    injector: Injector,
    private _housingService: HousingServiceProxy,
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

    this._housingService
      .getAll(request.keyword, true, request.skipCount, request.maxResultCount)
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: HousingDtoPagedResultDto) => {
        this.housings = result.items;
        this.showPaging(result, pageNumber);
      });
  }

  delete(housing: HousingDto): void {
    abp.message.confirm(
      this.l('HousingDeleteWarningMessage', housing.block + ' ' + housing.apartment),
      undefined,
      (result: boolean) => {
        if (result) {
          this._housingService
            .delete(housing.id)
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

  createHousing(): void {
    this.showCreateOrEditHousingDialog();
  }

  editHousing(housing: HousingDto): void {
    this.showCreateOrEditHousingDialog(housing.id);
  }

  accountActivities(housing: HousingDto): void {
    let accountActivitiesDialog: BsModalRef;
    accountActivitiesDialog = this._modalService.show(
      AccountActivitiesDialogComponent,
      {
        class: 'modal-lg, modal-xl',
        initialState: {
          id: housing.id
        }
      }
    );  }

  addPerson(housing: HousingDto): void {
    let addPersonDialog: BsModalRef;
    addPersonDialog = this._modalService.show(
      AddPersonDialogComponent,
      {
        class: 'modal-lg,',
        initialState: {
          id: housing.id
        }
      }
    );
  }

  showCreateOrEditHousingDialog(id?: string): void {
    let createOrEditHousingDialog: BsModalRef;
    if (!id) {
      createOrEditHousingDialog = this._modalService.show(
        CreateHousingDialogComponent,
        {
          class: 'modal-lg',
        }
      );
    } else {
      createOrEditHousingDialog = this._modalService.show(
        EditHousingDialogComponent,
        {
          class: 'modal-lg',
          initialState: {
            id: id,
          },
        }
      );
    }

    createOrEditHousingDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }
}
