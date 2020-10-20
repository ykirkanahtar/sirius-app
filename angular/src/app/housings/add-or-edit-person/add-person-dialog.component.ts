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
import { CreateHousingPersonDto, HousingDto, HousingPersonDto, HousingPersonType, HousingServiceProxy, LookUpDto } from '@shared/service-proxies/service-proxies';

@Component({
  templateUrl: 'add-person-dialog.component.html'
})
export class AddPersonDialogComponent extends AppComponentBase
  implements OnInit {
  saving = false;
  id: string;
  housing = new HousingDto();
  people: LookUpDto[];
  housingPerson = new HousingPersonDto();
  housingPersonId: string;
  housingPersonType = HousingPersonType;
  housingPersonTypeValues = Object.values(
    HousingPersonType
  ).filter((e) => typeof e === 'number');

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _housingService: HousingServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._housingService
      .get(this.id)
      .subscribe((result: HousingDto) => {
        this.housing = result;
      });

    this._housingService
      .getPeopleLookUp(this.id)
      .subscribe((result: LookUpDto[]) => {
        this.people = result;
      });
  }

  save(): void {
    this.saving = true;

    const housingPerson = new CreateHousingPersonDto();
    housingPerson.init(this.housing);

    housingPerson.housingId = this.housing.id;

    this._housingService
      .addPerson(this.housingPerson)
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
