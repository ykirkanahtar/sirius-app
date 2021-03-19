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
  UpdatePeriodDto,
} from "@shared/service-proxies/service-proxies";
import { CommonFunctions } from "@shared/helpers/CommonFunctions";

@Component({
  templateUrl: "edit-period-dialog.component.html",
})
export class EditPeriodDialogComponent
  extends AppComponentBase
  implements OnInit {
  saving = false;
  id: string;
  period = new PeriodDto();
  startDate: Date;
  endDate: Date;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _periodService: PeriodServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._periodService.get(this.id).subscribe((result: PeriodDto) => {
      this.period = result;
      this.startDate = this.period.startDate.toDate();

      if(this.period.endDate) {
        this.endDate = this.period.endDate.toDate();
      }
    });
  }

  save(): void {
    this.saving = true;

    const period = new UpdatePeriodDto();
    period.init(this.period);

    period.startDate = CommonFunctions.toMoment(this.startDate);

    if (this.endDate) {
      period.endDate = CommonFunctions.toMoment(this.endDate);
    }

    this._periodService
      .update(period)
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
