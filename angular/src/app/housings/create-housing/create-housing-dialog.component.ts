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
import { HousingDto, HousingServiceProxy, CreateHousingDto, LookUpDto, HousingCategoryServiceProxy } from '@shared/service-proxies/service-proxies';
import { result } from 'lodash';

@Component({
  templateUrl: 'create-housing-dialog.component.html'
})
export class CreateHousingDialogComponent extends AppComponentBase
  implements OnInit {
  saving = false;
  housing = new HousingDto();
  housingCategories: LookUpDto[];

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _housingService: HousingServiceProxy,
    private _housingCategoryService : HousingCategoryServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);

    this._housingCategoryService
      .getHousingCategoryLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.housingCategories = result;
      });
  }

  ngOnInit(): void {

  }

  save(): void {
    this.saving = true;

    const housing = new CreateHousingDto();
    housing.init(this.housing);

    this._housingService
      .create(housing)
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
