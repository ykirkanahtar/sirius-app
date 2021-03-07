import { Component, Injector, OnInit, ViewChild } from "@angular/core";
import { finalize } from "rxjs/operators";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { appModuleAnimation } from "@shared/animations/routerTransition";
import {
  PagedListingComponentBase,
  PagedRequestDto,
} from "@shared/paged-listing-component-base";
import {
  PeriodDto,
  PeriodServiceProxy,
  PeriodDtoPagedResultDto,
  LookUpDto,
  PeriodFor,
  BlockServiceProxy,
  PaymentCategoryServiceProxy,
  CreatePeriodForBlockDto,
  CreatePeriodForSiteDto,
  CreateHousingPaymentPlanGroupDto,
  HousingCategoryServiceProxy,
  HousingCategoryDto,
  PaymentAccountServiceProxy,
} from "@shared/service-proxies/service-proxies";
import { Table } from "primeng/table";
import { AppComponentBase } from "@shared/app-component-base";
import { CreateHousingPaymentPlanGroupForPeriodDialogComponent } from "./create-housing-payment-plan-group-for-period-dialog.component";
import { SelectItem } from "primeng/api";
import { Moment } from "moment";
import * as moment from "moment";
import { Router } from "@angular/router";
import { Input } from "hammerjs";

class PagedPeriodRequestDto extends PagedRequestDto {
  keyword: string;
}

enum PeriodType {
  Site,
  Block,
}

@Component({
  templateUrl: "create-period.component.html",
  animations: [appModuleAnimation()],
})
export class CreatePeriodComponent extends AppComponentBase implements OnInit {
  @ViewChild("dataTable", { static: true }) dataTable: Table;

  saving = false;
  public isTableLoading = false;
  public totalItems: number;

  period = new PeriodDto();
  periodFor: PeriodFor;
  periodForEnum = PeriodFor;
  blockItems = [];
  selectedBlock: string;
  housingPaymentPlanGroups: CreateHousingPaymentPlanGroupDto[] = [];
  paymentAccounts: LookUpDto[];
  defaultPaymentAccountId: string;

  paymentCategoriesFilter: SelectItem[] = [];
  selectedPaymentCategoriesFilter: string[] = [];

  housingCategoryMap = new Map<string, string>();

  constructor(
    injector: Injector,
    private _periodService: PeriodServiceProxy,
    private _paymentCategoryService: PaymentCategoryServiceProxy,
    private _paymentAccountService: PaymentAccountServiceProxy,
    private _housingCategoryService: HousingCategoryServiceProxy,
    private _blockService: BlockServiceProxy,
    private _router: Router,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._blockService.getBlockLookUp().subscribe((result: LookUpDto[]) => {
      this.blockItems = result;
    });

    this._paymentCategoryService
      .getPaymentCategoryForTransferLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.paymentCategoriesFilter = result;
      });

    this._paymentAccountService
      .getPaymentAccountLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.paymentAccounts = result;
      });
  }

  refresh(): void {}

  compareStartDates(): boolean {
    if (this.housingPaymentPlanGroups.length > 0) {
      let maxHousingPaymentPlanGroupStartDate = this.housingPaymentPlanGroups.reduce(
        (p, c) => (p.startDate > c.startDate ? p : c)
      );

      if (
        this.period.startDate > maxHousingPaymentPlanGroupStartDate.startDate
      ) {
        return true;
      }

      abp.message.error(
        this.l(
          "HousingPaymentPlanGroupStartDateCanNotBeSmallThanPeriodStartDate"
        ),
        this.l("Error")
      );
      return false;
    }

    return true;
  }

  selectHousingPaymentPlans() {
    if (!this.compareStartDates()) {
      return;
    }

    let createOrEditHousingPaymentPlanFoPeriodDialog = this._modalService.show(
      CreateHousingPaymentPlanGroupForPeriodDialogComponent,
      {
        class: "modal-lg",
        initialState: {
          periodStartDate: this.period.startDate,
        },
      }
    );

    createOrEditHousingPaymentPlanFoPeriodDialog.content.onSave.subscribe(
      (input: CreateHousingPaymentPlanGroupDto) => {
        this.housingPaymentPlanGroups.push(input);

        this._housingCategoryService
          .get(input.housingCategoryId)
          .subscribe((housingCategory: HousingCategoryDto) => {
            this.housingCategoryMap.set(
              input.housingCategoryId,
              housingCategory.housingCategoryName
            );
          });

        this.refresh();
      }
    );
  }

  delete(housingPaymentPlanGroup: CreateHousingPaymentPlanGroupDto): void {
    abp.message.confirm(
      this.l("DeleteWarningMessage"),
      undefined,
      (result: boolean) => {
        if (result) {
          this.housingPaymentPlanGroups.forEach((element, index) => {
            if (element == housingPaymentPlanGroup)
              this.housingPaymentPlanGroups.splice(index, 1);
          });
        }
      }
    );
  }

  clearFilters(): void {}

  getFormattedDate(date: Moment): string {
    return moment(date).format("DD-MM-YYYY");
  }

  getHousingCategoryName(housingCategoryId: string): any {
    return this.housingCategoryMap.get(housingCategoryId);
  }

  save(): void {
    if (!this.compareStartDates()) {
      return;
    }
    
    this.saving = true;

    // if (this.periodFor === PeriodFor.Site) {
    const period = new CreatePeriodForSiteDto();
    period.init(this.period);
    period.paymentCategories = this.selectedPaymentCategoriesFilter;
    period.housingPaymentPlanGroups = this.housingPaymentPlanGroups;
    period.defaultPaymentAccountIdForRegularHousingDue = this.defaultPaymentAccountId;

    this._periodService
      .createForSite(period)
      .pipe(
        finalize(() => {
          this.saving = false;
        })
      )
      .subscribe(() => {
        this.notify.info(this.l("SavedSuccessfully"));
        this._router.navigate(["/app/site-periods"]);
      });
    // } else if(this.periodFor === PeriodFor.Block) {
    //   const period = new CreatePeriodForBlockDto();
    //   period.init(this.period);
    //   this.period.blockId = this.selectedBlock;

    //   this._periodService
    //     .createForBlock(period)
    //     .pipe(
    //       finalize(() => {
    //         this.saving = false;
    //       })
    //     )
    //     .subscribe(() => {
    //       this.notify.info(this.l("SavedSuccessfully"));
    //       //Todo go to periods page
    //     });
    // } else {
    //   abp.message.error("InvalidPeriodType", "Error");
    // }
  }
}
