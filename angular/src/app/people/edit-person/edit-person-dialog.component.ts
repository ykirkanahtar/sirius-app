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
import { PersonDto, PersonServiceProxy } from '@shared/service-proxies/service-proxies';

@Component({
  templateUrl: 'edit-person-dialog.component.html'
})
export class EditPersonDialogComponent extends AppComponentBase
  implements OnInit {
  saving = false;
  id: string;
  person = new PersonDto();

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _personService: PersonServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._personService
      .get(this.id)
      .subscribe((result: PersonDto) => {
          this.person = result;
      });
  }

  save(): void {
    this.saving = true;

    const person = new PersonDto();
    person.init(this.person);

    this._personService
      .update(person)
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
