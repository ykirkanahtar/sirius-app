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
import { PersonDto, PersonServiceProxy, CreatePersonDto } from '@shared/service-proxies/service-proxies';

@Component({
  templateUrl: 'create-person-dialog.component.html'
})
export class CreatePersonDialogComponent extends AppComponentBase
  implements OnInit {
  saving = false;
  person = new PersonDto();

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _peopleervice: PersonServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {

  }

  save(): void {
    this.saving = true;

    const person = new CreatePersonDto();
    person.init(this.person);

    this._peopleervice
      .create(person)
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
