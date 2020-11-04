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
import { BlockServiceProxy, HousingCategoryServiceProxy, HousingDto, HousingServiceProxy, LookUpDto } from '@shared/service-proxies/service-proxies';

@Component({
  templateUrl: 'edit-housing-dialog.component.html'
})
export class EditHousingDialogComponent extends AppComponentBase
  implements OnInit {
  saving = false;
  id: string;
  housing = new HousingDto();
  housingCategories: LookUpDto[];
  blocks: LookUpDto[];

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _housingService: HousingServiceProxy,
    private _housingCategoryService: HousingCategoryServiceProxy,
    private _blockService: BlockServiceProxy,
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
    this._housingService
      .get(this.id)
      .subscribe((result: HousingDto) => {
        this.housing = result;
      });

    this._blockService
      .getBlockLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.blocks = result;
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
