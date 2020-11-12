import { Component, Injector, OnInit } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {
  HousingPaymentPlanServiceProxy,
  CreateDebtHousingPaymentPlanForHousingCategoryDto,
  HousingCategoryServiceProxy,
  LookUpDto,
} from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/app-component-base';
import * as moment from 'moment';

@Component({
  templateUrl: './create-housing-payment-plan.component.html',
  animations: [appModuleAnimation()],
})
export class CreateHousingPaymentPlanComponent
  extends AppComponentBase
  implements OnInit {
  saving = false;
  input = new CreateDebtHousingPaymentPlanForHousingCategoryDto();
  housingCategories: LookUpDto[];

  constructor(
    injector: Injector,
    private _housingPaymentPlanService: HousingPaymentPlanServiceProxy,
    private _housingCategoryService: HousingCategoryServiceProxy
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._housingCategoryService
      .getHousingCategoryLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.housingCategories = result;
      });
  }

  save(): void {
    console.log(this.input);
    this.saving = true;
    this._housingPaymentPlanService
      .createDebtPaymentForHousingCategory(this.input)
      .pipe(
        finalize(() => {
          this.saving = false;
        })
      )
      .subscribe(() => {
        this.notify.info(this.l('SavedSuccessfully'));
      });
  }
}
