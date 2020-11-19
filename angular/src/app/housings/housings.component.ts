import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {
  PagedListingComponentBase,
  PagedRequestDto,
} from '@shared/paged-listing-component-base';
import { CreateHousingDialogComponent } from './create-housing/create-housing-dialog.component';
import { EditHousingDialogComponent } from './edit-housing/edit-housing-dialog.component';
import {
  HousingDto,
  HousingServiceProxy,
  HousingDtoPagedResultDto,
  HousingCategoryServiceProxy,
  LookUpDto,
  PersonServiceProxy,
} from '@shared/service-proxies/service-proxies';
import { AddPersonDialogComponent } from './add-or-edit-person/add-person-dialog.component';
import { AccountActivitiesDialogComponent } from './account-activities/account-activities.component';
import { LazyLoadEvent, SelectItem } from 'primeng/api';
import { Table } from 'primeng/table';
import { HousingPeopleDialogComponent } from './housing-people/housing-people.component';

class PagedHousingsRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: './housings.component.html',
  animations: [appModuleAnimation()],
})
export class HousingsComponent
  extends PagedListingComponentBase<HousingDto>
  implements OnInit {
  @ViewChild('dataTable', { static: true }) dataTable: Table;

  sortingColumn: string;
  advancedFiltersVisible = false;

  housings: HousingDto[] = [];

  housingCategoriesFilter: SelectItem[] = [];
  selectedHousingCategoriesFilter: string[] = [];

  housingsFilters: SelectItem[] = [];
  selectedHousingsFilters: string[] = [];

  peopleFilters: SelectItem[] = [];
  selectedPeopleFilters: string[] = [];

  constructor(
    injector: Injector,
    private _housingService: HousingServiceProxy,
    private _housingCategoryService: HousingCategoryServiceProxy,
    private _personService: PersonServiceProxy,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._housingCategoryService
      .getHousingCategoryLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.housingCategoriesFilter = result;
      });

    this._housingService.getHousingLookUp().subscribe((result: LookUpDto[]) => {
      this.housingsFilters = result;
    });

    this._personService.getPersonLookUp().subscribe((result: LookUpDto[]) => {
      this.peopleFilters = result;
    });

    this.getDataPage(1);
  }

  createHousing(): void {
    this.showCreateOrEditHousingDialog();
  }

  editHousing(housing: HousingDto): void {
    this.showCreateOrEditHousingDialog(housing.id);
  }

  clearFilters(): void {
    this.selectedHousingCategoriesFilter = [];
    this.selectedHousingsFilters = [];
    this.selectedPeopleFilters = [];
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
    this._housingService
      .getAll(
        this.selectedHousingsFilters,
        this.selectedHousingCategoriesFilter,
        this.selectedPeopleFilters,
        this.sortingColumn,
        request.skipCount,
        request.maxResultCount
      )
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

  protected delete(housing: HousingDto): void {
    abp.message.confirm(
      this.l(
        'HousingDeleteWarningMessage',
        housing.block + ' ' + housing.apartment
      ),
      undefined,
      (result: boolean) => {
        if (result) {
          this._housingService.delete(housing.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  protected getAccountActivities(housing: HousingDto): void {
    let accountActivitiesDialog: BsModalRef;
    accountActivitiesDialog = this._modalService.show(
      AccountActivitiesDialogComponent,
      {
        class: 'modal-lg, modal-xl',
        initialState: {
          id: housing.id,
        },
      }
    );
  }

  protected getPeople(housing: HousingDto): void {
    let housingPeopleDialog: BsModalRef;
    housingPeopleDialog = this._modalService.show(
      HousingPeopleDialogComponent,
      {
        class: 'modal-lg, modal-xl',
        initialState: {
          id: housing.id,
        },
      }
    );
  }

  protected addPerson(housing: HousingDto): void {
    let addPersonDialog: BsModalRef;
    addPersonDialog = this._modalService.show(AddPersonDialogComponent, {
      class: 'modal-lg,',
      initialState: {
        id: housing.id,
      },
    });
  }

  private showCreateOrEditHousingDialog(id?: string): void {
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
