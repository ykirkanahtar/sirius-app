import {
  Component,
  Injector,
  OnInit,
  EventEmitter,
  Output,
  AfterViewInit,
  AfterViewChecked
} from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef } from 'ngx-bootstrap/modal';
import * as _ from 'lodash';
import { AppComponentBase } from '@shared/app-component-base';
import {
  HousingServiceProxy,
  CreateHousingDto,
  LookUpDto,
  HousingCategoryServiceProxy,
  BlockServiceProxy,
  CreateTransferForHousingDueDto,
} from '@shared/service-proxies/service-proxies';
import { Moment } from 'moment';
import * as moment from 'moment';

@Component({
  templateUrl: 'create-housing-dialog.component.html'
})
export class CreateHousingDialogComponent extends AppComponentBase
  implements OnInit {
  saving = false;
  housing = new CreateHousingDto();
  housingCategories: LookUpDto[];
  blocks: LookUpDto[];
  isCredit: boolean;
  dateForDatepicker = moment();

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _housingService: HousingServiceProxy,
    private _housingCategoryService: HousingCategoryServiceProxy,
    private _blockService: BlockServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._housingCategoryService
      .getHousingCategoryLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.housingCategories = result;
        if (this.housingCategories.length === 1) {
          this.housing.housingCategoryId = this.housingCategories[0].value;
        }
      });

    this._blockService
      .getBlockLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.blocks = result;
      });

    this.housing.createTransferForHousingDue = new CreateTransferForHousingDueDto();
    this.housing.createTransferForHousingDue.isDebt = true;
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
