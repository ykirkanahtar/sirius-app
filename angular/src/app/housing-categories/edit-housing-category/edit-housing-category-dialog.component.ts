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
import { HousingCategoryDto, HousingCategoryServiceProxy } from '@shared/service-proxies/service-proxies';

@Component({
  templateUrl: 'edit-housing-category-dialog.component.html'
})
export class EditHousingCategoryDialogComponent extends AppComponentBase
  implements OnInit {
  saving = false;
  id: string;
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
    this._housingCategoryService
      .get(this.id)
      .subscribe((result: HousingCategoryDto) => {
        this.housingCategory = result;
      });
  }

  save(): void {
    this.saving = true;

    const housingCategory = new HousingCategoryDto();
    housingCategory.init(this.housingCategory);

    this._housingCategoryService
      .update(housingCategory)
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
