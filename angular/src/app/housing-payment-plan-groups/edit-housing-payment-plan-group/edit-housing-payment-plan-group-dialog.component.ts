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
import {
  UpdateHousingPaymentPlanGroupDto,
  HousingPaymentPlanGroupServiceProxy,
  HousingPaymentPlanGroupDto,
} from '@shared/service-proxies/service-proxies';

@Component({
  templateUrl: './edit-housing-payment-plan-group-dialog.component.html',
})
export class EditHousingPaymentPlanGroupDialogComponent extends AppComponentBase
  implements OnInit {
  saving = false;
  id: string;
  housingPaymentPlanGroup = new HousingPaymentPlanGroupDto();

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _housingPaymentPlanGroupService: HousingPaymentPlanGroupServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._housingPaymentPlanGroupService
      .get(this.id)
      .subscribe((result: HousingPaymentPlanGroupDto) => {
        this.housingPaymentPlanGroup = result;
      });
  }

  save(): void {
    this.saving = true;

    const housingPaymentPlanGroup = new UpdateHousingPaymentPlanGroupDto();
    housingPaymentPlanGroup.init(this.housingPaymentPlanGroup);

    this._housingPaymentPlanGroupService
      .update(housingPaymentPlanGroup)
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
