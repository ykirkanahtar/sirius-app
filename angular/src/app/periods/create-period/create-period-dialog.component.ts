import {
  Component,
  Injector,
  OnInit,
  EventEmitter,
  Output,
} from "@angular/core";
import { finalize } from "rxjs/operators";
import { BsModalRef } from "ngx-bootstrap/modal";
import * as _ from "lodash";
import { AppComponentBase } from "@shared/app-component-base";
import {
  PeriodDto,
  PeriodServiceProxy,
  CreatePeriodForSiteDto,
  CreatePeriodForBlockDto,
  SiteOrBlock,
  BlockServiceProxy,
  LookUpDto,
  PaymentCategoryServiceProxy,
} from "@shared/service-proxies/service-proxies";
import { SelectItem } from "primeng/api";
import * as moment from "moment";
import { CommonFunctions } from "@shared/helpers/CommonFunctions";

@Component({
  templateUrl: "create-period-dialog.component.html",
})
export class CreatePeriodDialogComponent
  extends AppComponentBase
  implements OnInit {
  saving = false;
  period = new PeriodDto();
  siteOrBlock: SiteOrBlock;
  siteOrBlockEnum = SiteOrBlock;
  blockItems = [];
  selectedBlock: string;
  startDate: Date;
  endDate: Date;

  paymentCategoriesFilter: SelectItem[] = [];
  selectedPaymentCategoriesFilter: string[] = [];

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _periodService: PeriodServiceProxy,
    private _paymentCategoryService: PaymentCategoryServiceProxy,
    private _blockService: BlockServiceProxy,
    public bsModalRef: BsModalRef
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

    this.startDate = abp.clock.now();
  }

  save(): void {
    this.saving = true;

    if (this.siteOrBlock === SiteOrBlock.Site) {
      const period = new CreatePeriodForSiteDto();
      period.init(this.period);
      period.paymentCategories = this.selectedPaymentCategoriesFilter;

      period.startDateString = CommonFunctions.dateToString(this.startDate);
      if (this.endDate !== undefined && this.endDate) {
        period.endDateString = CommonFunctions.dateToString(this.endDate);
      }

      this._periodService
        .createForSite(period)
        .pipe(
          finalize(() => {
            this.saving = false;
          })
        )
        .subscribe(() => {
          this.notify.info(this.l("SavedSuccessfully"));
          this.bsModalRef.hide();
          this.onSave.emit();
        });
    } else {
      const period = new CreatePeriodForBlockDto();
      period.init(this.period);
      this.period.blockId = this.selectedBlock;

      this._periodService
        .createForBlock(period)
        .pipe(
          finalize(() => {
            this.saving = false;
          })
        )
        .subscribe(() => {
          this.notify.info(this.l("SavedSuccessfully"));
          this.bsModalRef.hide();
          this.onSave.emit();
        });
    }
  }
}
