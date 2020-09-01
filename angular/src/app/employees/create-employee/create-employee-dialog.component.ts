import {
  Component,
  Injector,
  OnInit,
  EventEmitter,
  Output
} from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef } from 'ngx-bootstrap/modal';
import * as _ from 'lodash';
import { AppComponentBase } from '@shared/app-component-base';
import { EmployeeDto, EmployeeServiceProxy, CreateEmployeeDto } from '@shared/service-proxies/service-proxies';

@Component({
  templateUrl: 'create-employee-dialog.component.html'
})
export class CreateEmployeeDialogComponent extends AppComponentBase
  implements OnInit {
  saving = false;
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

  }

  save(): void {
    this.saving = true;

    const employee = new CreateEmployeeDto();
    employee.init(this.employee);

    this._employeeService
      .create(employee)
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
