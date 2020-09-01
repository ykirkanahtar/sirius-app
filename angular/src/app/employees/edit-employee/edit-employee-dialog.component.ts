import {
  Component,
  Injector,
  OnInit,
  EventEmitter,
  Output,
} from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef } from 'ngx-bootstrap/modal';
import * as _ from 'lodash';
import { AppComponentBase } from '@shared/app-component-base';
import { EmployeeDto, EmployeeServiceProxy } from '@shared/service-proxies/service-proxies';


@Component({
  templateUrl: 'edit-employee-dialog.component.html'
})
export class EditEmployeeDialogComponent extends AppComponentBase
  implements OnInit {
  saving = false;
  id: string;
  employee = new EmployeeDto();

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _employeeService: EmployeeServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._employeeService
      .get(this.id)
      .subscribe((result: EmployeeDto) => {
          this.employee = result;
      });
  }

  save(): void {
    this.saving = true;

    const employee = new EmployeeDto();
    employee.init(this.employee);

    this._employeeService
      .update(employee)
      .pipe(
        finalize(() => {
          this.saving = false;
        })
      )
      .subscribe(() => {
        this.notify.info(this.l('SavedSuccessfully'));
        this.bsModalRef.hide();
        this.onSave.emit();
      });
  }
}
