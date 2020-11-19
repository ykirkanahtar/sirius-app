import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {
  PagedListingComponentBase,
  PagedRequestDto,
} from '@shared/paged-listing-component-base';
import { CreatePersonDialogComponent } from './create-person/create-person-dialog.component';
import { EditPersonDialogComponent } from './edit-person/edit-person-dialog.component';
import {
  PersonDto,
  PersonServiceProxy,
  PersonDtoPagedResultDto,
  HousingServiceProxy,
  LookUpDto,
} from '@shared/service-proxies/service-proxies';
import { LazyLoadEvent, SelectItem } from 'primeng/api';
import { Table } from 'primeng/table';

class PagedPeopleRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: './people.component.html',
  animations: [appModuleAnimation()],
})
export class PeopleComponent
  extends PagedListingComponentBase<PersonDto>
  implements OnInit {
  @ViewChild('dataTable', { static: true }) dataTable: Table;

  sortingColumn: string;
  advancedFiltersVisible = false;

  people: PersonDto[] = [];

  peopleFilter: string[] = [];
  selectedPersonFilter: string;

  housingsFilter: SelectItem[] = [];
  selectedHousingsFilter: string[] = [];

  phoneNumberFilter: string;

  constructor(
    injector: Injector,
    private _peopleService: PersonServiceProxy,
    private _housingService: HousingServiceProxy,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._housingService.getHousingLookUp().subscribe((result: LookUpDto[]) => {
      this.housingsFilter = result;
    });

    this.getDataPage(1);
  }

  searchPeople(event) {
    this._peopleService
      .getPeopleFromAutoCompleteFilter(event.query)
      .subscribe((result) => {
        this.peopleFilter = result;
      });
  }

  createPerson(): void {
    this.showCreateOrEditPersonDialog();
  }

  editPerson(person: PersonDto): void {
    this.showCreateOrEditPersonDialog(person.id);
  }

  clearFilters(): void {
    this.peopleFilter = [];
    this.selectedPersonFilter = '';
    this.phoneNumberFilter = '';
    this.selectedHousingsFilter = [];
    this.getDataPage(1);
  }

  getData(event?: LazyLoadEvent) {
    this.sortingColumn = this.primengTableHelper.getSorting(this.dataTable);
    this.getDataPage(1);
  }

  protected list(
    request: PagedPeopleRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    this._peopleService
      .getAll(
        this.selectedPersonFilter,
        this.phoneNumberFilter,
        this.selectedHousingsFilter,
        this.sortingColumn,
        request.skipCount,
        request.maxResultCount
      )
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: PersonDtoPagedResultDto) => {
        this.people = result.items;
        this.showPaging(result, pageNumber);
      });
  }

  protected delete(person: PersonDto): void {
    abp.message.confirm(
      this.l(
        'PersonDeleteWarningMessage',
        person.firstName + ' ' + person.lastName
      ),
      undefined,
      (result: boolean) => {
        if (result) {
          this._peopleService.delete(person.id).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }

  private showCreateOrEditPersonDialog(id?: string): void {
    let createOrEditPersonDialog: BsModalRef;
    if (!id) {
      createOrEditPersonDialog = this._modalService.show(
        CreatePersonDialogComponent,
        {
          class: 'modal-lg',
        }
      );
    } else {
      createOrEditPersonDialog = this._modalService.show(
        EditPersonDialogComponent,
        {
          class: 'modal-lg',
          initialState: {
            id: id,
          },
        }
      );
    }

    createOrEditPersonDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }
}
