import {
  Component,
  Injector,
  ViewChild,
  OnInit,
} from "@angular/core";
import { BsModalRef } from "ngx-bootstrap/modal";
import * as _ from "lodash";
import { appModuleAnimation } from "@shared/animations/routerTransition";
import { Table } from "primeng/table";
import { AppComponentBase } from "@shared/app-component-base";
import {
  LookUpDto,
  HousingPaymentPlanGroupForHousingCategoryDto,
  HousingPaymentPlanGroupForHousingDto,
} from "@shared/service-proxies/service-proxies";

@Component({
  templateUrl:
    "./housing-payment-plan-group-amounts.component.html",
  animations: [appModuleAnimation()],
})
export class HousingPaymentPlanGroupAmountsComponent
  extends AppComponentBase {
  @ViewChild("dataTable", { static: true }) dataTable: Table;

  isTableLoading: boolean = false;

  housingPaymentPlanGroupName: string;
  housingPaymentPlanGroupForHousingCategories: HousingPaymentPlanGroupForHousingCategoryDto[] = [];
  housingPaymentPlanGroupForHousings: HousingPaymentPlanGroupForHousingDto[] = [];
  housings: LookUpDto[];
  housingCategories: LookUpDto[];

  constructor(
    injector: Injector,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  getHousingCategoryName(housingCategoryId: string): string {
    return this.housingCategories.filter(p => p.value === housingCategoryId).map(p => p.label)[0];
  }

  getHousingName(housingId: string): string {
    return this.housings.filter(p => p.value === housingId).map(p => p.label)[0];
  }
}
