import { Component, Injector } from '@angular/core';
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

class PagedEmployeesRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: './Employees.component.html',
  animations: [appModuleAnimation()]
})
export class EmployeesComponent extends PagedListingComponentBase<EmployeeDto> {
  employees: EmployeeDto[] = [];
  keyword = '';

  constructor(
    injector: Injector,
    private _employeesService: EmployeeServiceProxy,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  list(
    request: PagedEmployeesRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;

    this._employeesService
      .getAll(request.keyword, true, request.skipCount, request.maxResultCount)
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

  delete(employee: EmployeeDto): void {
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
            .subscribe(() => {});
        }
      }
    );
  }

  createEmployee(): void {
    this.showCreateOrEditEmployeeDialog();
  }

  editEmployee(employee: EmployeeDto): void {
    this.showCreateOrEditEmployeeDialog(employee.id);
  }

  showCreateOrEditEmployeeDialog(id?: string): void {
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
