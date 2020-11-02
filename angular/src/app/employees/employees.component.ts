import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {
  PagedListingComponentBase,
  PagedRequestDto
} from '@shared/paged-listing-component-base';
import { CreateEmployeeDialogComponent } from './create-employee/create-employee-dialog.component';
import { EditEmployeeDialogComponent } from './edit-employee/edit-employee-dialog.component';
import { EmployeeDto, EmployeeServiceProxy, EmployeeDtoPagedResultDto } from '@shared/service-proxies/service-proxies';
import { Table } from 'primeng/table';
import { LazyLoadEvent } from 'primeng/api';

class PagedEmployeesRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: './Employees.component.html',
  animations: [appModuleAnimation()]
})
export class EmployeesComponent extends PagedListingComponentBase<EmployeeDto>
  implements OnInit {

  @ViewChild('dataTable', { static: true }) dataTable: Table;

  sortingColumn: string;
  advancedFiltersVisible = false;

  employees: EmployeeDto[] = [];

  employeesFilter: string[] = [];
  selectedEmployeeFilter: string;

  phoneNumberFilter: string;

  constructor(
    injector: Injector,
    private _employeesService: EmployeeServiceProxy,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.getDataPage(1);
  }

  searchEmployee(event) {
    this._employeesService
      .getEmployeeFromAutoCompleteFilter(event.query)
      .subscribe((result) => {
        this.employeesFilter = result;
      });
  }

  createEmployee(): void {
    this.showCreateOrEditEmployeeDialog();
  }

  editEmployee(employee: EmployeeDto): void {
    this.showCreateOrEditEmployeeDialog(employee.id);
  }

  clearFilters(): void {
    this.employeesFilter = [];
    this.selectedEmployeeFilter = '';
    this.phoneNumberFilter = '';
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
    this._employeesService
      .getAll(
        this.selectedEmployeeFilter,
        this.phoneNumberFilter,
        this.sortingColumn,
        request.skipCount,
        request.maxResultCount)
      .pipe(
          finalize(() => {
            finishedCallback();
          })
        )
      .subscribe((result: EmployeeDtoPagedResultDto) => {
        this.employees = result.items;
        this.showPaging(result, pageNumber);
      });
  }

  protected delete(employee: EmployeeDto): void {
    abp.message.confirm(
      this.l('EmployeeDeleteWarningMessage', employee.firstName + ' ' + employee.lastName),
      undefined,
      (result: boolean) => {
        if (result) {
          this._employeesService
            .delete(employee.id)
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

  private showCreateOrEditEmployeeDialog(id?: string): void {
    let createOrEditEmployeeDialog: BsModalRef;
    if (!id) {
      createOrEditEmployeeDialog = this._modalService.show(
        CreateEmployeeDialogComponent,
        {
          class: 'modal-lg',
        }
      );
    } else {
      createOrEditEmployeeDialog = this._modalService.show(
        EditEmployeeDialogComponent,
        {
          class: 'modal-lg',
          initialState: {
            id: id,
          },
        }
      );
    }

    createOrEditEmployeeDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }
}
