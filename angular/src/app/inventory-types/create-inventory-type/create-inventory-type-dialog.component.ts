import {
  Component,
  Injector,
  EventEmitter,
  Output,
  OnInit,
} from "@angular/core";
import { finalize } from "rxjs/operators";
import { BsModalRef } from "ngx-bootstrap/modal";
import * as _ from "lodash";
import { AppComponentBase } from "@shared/app-component-base";
import {
  CreateInventoryTypeDto,
  InventoryTypeServiceProxy,
  QuantityType,
} from "@shared/service-proxies/service-proxies";

@Component({
  templateUrl: "create-inventory-type-dialog.component.html",
})
export class CreateInventoryTypeDialogComponent
  extends AppComponentBase
  implements OnInit {
  saving = false;
  input = new CreateInventoryTypeDto();

  quantityTypes: any[];
  quantityTypeEnum = QuantityType;

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _inventoryTypeService: InventoryTypeServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.quantityTypes = [
      {
        value: QuantityType.Piece.toString(),
        label: this.l(this.quantityTypeEnum[QuantityType.Piece]),
      },
      {
        value: QuantityType.Kilogram.toString(),
        label: this.l(this.quantityTypeEnum[QuantityType.Kilogram]),
      },
      {
        value: QuantityType.Litre.toString(),
        label: this.l(this.quantityTypeEnum[QuantityType.Litre]),
      },      
    ];
  }

  save(): void {
    this.saving = true;

    this._inventoryTypeService
      .create(this.input)
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
