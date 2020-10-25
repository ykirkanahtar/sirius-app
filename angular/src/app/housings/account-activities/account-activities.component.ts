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
import { HousingDto, HousingPaymentPlanDto, HousingPaymentPlanDtoPagedResultDto, HousingPaymentPlanServiceProxy, HousingServiceProxy, PaymentPlanType } from '@shared/service-proxies/service-proxies';
import {
  PagedListingComponentBase,
  PagedRequestDto
} from '@shared/paged-listing-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';

class PagedHousingPaymentPlanResultRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: './account-activities.component.html',
  animations: [appModuleAnimation()]
})
export class AccountActivitiesDialogComponent extends PagedListingComponentBase<HousingDto>
{

  housingPaymentPlans: HousingPaymentPlanDto[] = [];
  housing = new HousingDto();
  keyword = '';
  paymentPlanTypeEnum = PaymentPlanType;

  id: string;
  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _housingService: HousingServiceProxy,
    private _housingPaymentPlanService: HousingPaymentPlanServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  list(
    request: PagedHousingPaymentPlanResultRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {

    this._housingService
      .get(this.id)
      .subscribe((result: HousingDto) => {
        this.housing = result;
      });

    request.keyword = this.keyword;

    this._housingPaymentPlanService
      .getAllByHousingId(this.id, request.keyword, true, request.skipCount, request.maxResultCount)
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: HousingPaymentPlanDtoPagedResultDto) => {
        this.housingPaymentPlans = result.items;
        this.showPaging(result, pageNumber);
      });
  }

  delete(): void {

  }
}
