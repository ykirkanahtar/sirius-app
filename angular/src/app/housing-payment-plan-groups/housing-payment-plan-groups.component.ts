import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {
  PagedListingComponentBase,
  PagedRequestDto,
} from '@shared/paged-listing-component-base';
import {
  HousingServiceProxy,
  HousingCategoryServiceProxy,
  LookUpDto,
  PersonServiceProxy,
  HousingPaymentPlanGroupDto,
  HousingPaymentPlanGroupServiceProxy,
  HousingPaymentPlanGroupDtoPagedResultDto,
  ResidentOrOwner,
} from '@shared/service-proxies/service-proxies';
import { LazyLoadEvent, SelectItem } from 'primeng/api';
import { Table } from 'primeng/table';
import { CreateHousingPaymentPlanGroupDialogComponent } from './create-housing-payment-plan-group/create-housing-payment-plan-group-dialog.component';
import { EditHousingPaymentPlanGroupDialogComponent } from './edit-housing-payment-plan-group/edit-housing-payment-plan-group-dialog.component';

class PagedHousingsRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: './housing-payment-plan-groups.component.html',
  animations: [appModuleAnimation()],
})
export class HousingPaymentPlanGroupsComponent
  extends PagedListingComponentBase<HousingPaymentPlanGroupDto>
  implements OnInit {
  @ViewChild('dataTable', { static: true }) dataTable: Table;

  sortingColumn: string;
  advancedFiltersVisible = false;
  residentOrOwnerEnum = ResidentOrOwner;

  housingPaymentPlanGroups: HousingPaymentPlanGroupDto[] = [];

  housingCategoriesFilter: SelectItem[] = [];
  selectedHousingCategoriesFilter: string[] = [];

  housingsFilters: SelectItem[] = [];
  selectedHousingsFilters: string[] = [];

  peopleFilters: SelectItem[] = [];
  selectedPeopleFilters: string[] = [];

  constructor(
    injector: Injector,
    private _housingPaymentPlanGroupService: HousingPaymentPlanGroupServiceProxy,
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

    this._housingService.getHousingLookUp(undefined, undefined).subscribe((result: LookUpDto[]) => {
      this.housingsFilters = result;
    });

    this._personService.getPersonLookUp().subscribe((result: LookUpDto[]) => {
      this.peopleFilters = result;
    });

    this.getDataPage(1);
  }

  createHousingPaymentPlanGroup(): void {
    this.showCreateOrEditHousingPaymentPlanGroupDialog();
  }

  editHousingPaymentPlanGroup(housingPaymentPlanGroup: HousingPaymentPlanGroupDto): void {
    this.showCreateOrEditHousingPaymentPlanGroupDialog(housingPaymentPlanGroup.id);
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
    this._housingPaymentPlanGroupService
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
      .subscribe((result: HousingPaymentPlanGroupDtoPagedResultDto) => {
        this.housingPaymentPlanGroups = result.items;
        this.showPaging(result, pageNumber);
      });
  }

  protected delete(housingPaymentPlanGroup: HousingPaymentPlanGroupDto): void {
    abp.message.confirm(
      this.l(
        'HousingPaymentPlanGroupDeleteWarningMessage',
        housingPaymentPlanGroup.housingPaymentPlanGroupName
      ),
      undefined,
      (result: boolean) => {
        if (result) {
          this._housingPaymentPlanGroupService.delete(housingPaymentPlanGroup.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  private showCreateOrEditHousingPaymentPlanGroupDialog(id?: string): void {
    let createOrEditHousingPaymentPlanDialog: BsModalRef;
    if (!id) {
      createOrEditHousingPaymentPlanDialog = this._modalService.show(
        CreateHousingPaymentPlanGroupDialogComponent,
        {
          class: 'modal-lg',
        }
      );
    } else {
      createOrEditHousingPaymentPlanDialog = this._modalService.show(
        EditHousingPaymentPlanGroupDialogComponent,
        {
          class: 'modal-lg',
          initialState: {
            id: id,
          },
        }
      );
    }

    createOrEditHousingPaymentPlanDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }
}
