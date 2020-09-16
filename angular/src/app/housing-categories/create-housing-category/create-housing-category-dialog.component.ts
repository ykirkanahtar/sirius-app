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
import { HousingCategoryDto, HousingCategoryServiceProxy, CreateHousingCategoryDto } from '@shared/service-proxies/service-proxies';

@Component({
  templateUrl: 'create-housing-category-dialog.component.html'
})
export class CreateHousingCategoryDialogComponent extends AppComponentBase
  implements OnInit {
  saving = false;
  housingCategory = new HousingCategoryDto();

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _housingCategoryService: HousingCategoryServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {

  }

  save(): void {
    this.saving = true;

    const housingCategory = new CreateHousingCategoryDto();
    housingCategory.init(this.housingCategory);

    this._housingCategoryService
      .create(housingCategory)
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
