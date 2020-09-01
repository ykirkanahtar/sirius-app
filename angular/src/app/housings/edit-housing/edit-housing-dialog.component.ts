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
import { HousingDto, HousingServiceProxy } from '@shared/service-proxies/service-proxies';

@Component({
  templateUrl: 'edit-housing-dialog.component.html'
})
export class EditHousingDialogComponent extends AppComponentBase
  implements OnInit {
  saving = false;
  id: string;
  housing = new HousingDto();

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
  }

  save(): void {
    this.saving = true;

    const housing = new HousingDto();
    housing.init(this.housing);

    this._housingService
      .update(housing)
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
