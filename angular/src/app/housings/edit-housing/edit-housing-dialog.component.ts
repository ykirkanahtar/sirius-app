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
  BlockServiceProxy,
  HousingCategoryServiceProxy,
  HousingServiceProxy,
  LookUpDto,
  ResidentOrOwner,
  UpdateHousingDto,
} from "@shared/service-proxies/service-proxies";
import { CommonFunctions } from "@shared/helpers/CommonFunctions";
import * as moment from "moment";

@Component({
  templateUrl: "edit-housing-dialog.component.html",
})
export class EditHousingDialogComponent
  extends AppComponentBase
  implements OnInit {
  saving = false;
  id: string;
  housing = new UpdateHousingDto();
  housingCategories: LookUpDto[];
  blocks: LookUpDto[];
  oldTenantIsResidingValue = false;
  residentOrOwners: any[];
  residentOrOwnerEnum = ResidentOrOwner;
  transferDate: Date;
  residentOrOwner: string;

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

    this.residentOrOwners = [
      {
        value: ResidentOrOwner.Resident.toString(),
        label: this.l(this.residentOrOwnerEnum[ResidentOrOwner.Resident]),
      },
      {
        value: ResidentOrOwner.Owner.toString(),
        label: this.l(this.residentOrOwnerEnum[ResidentOrOwner.Owner]),
      },
    ];
    
    this._housingService
      .getHousingForUpdate(this.id)
      .subscribe((result: UpdateHousingDto) => {
        this.housing = result;
        if (result.transferAmount && result.transferAmount != 0) {
          this.transferDate = this.housing.transferDate.toDate();
          this.residentOrOwner = this.housing.transferIsForResidentOrOwner === ResidentOrOwner.Resident ?
          ResidentOrOwner.Resident.toString() : 
          ResidentOrOwner.Owner.toString();
        } else {
          this.residentOrOwner = ResidentOrOwner.Resident.toString();
          this.transferDate = moment().toDate();

        }
        this.oldTenantIsResidingValue = this.housing.tenantIsResiding;
      });

    this._blockService.getBlockLookUp().subscribe((result: LookUpDto[]) => {
      this.blocks = result;
    });
  }

  deleteTransferForHousingDue(): void {
    if(this.housing.deleteTransferForHousingDue) {
      this.housing.transferAmount = 0;
    }
  }

  save(): void {
    this.saving = true;

    if (this.housing.transferAmount != 0) {
      this.housing.transferDate = CommonFunctions.toMoment(
        this.transferDate
      );
      this.housing.transferIsForResidentOrOwner = this.residentOrOwner === ResidentOrOwner.Resident.toString() ? 1 : 2;
    }

    this._housingService
      .update(this.housing)
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