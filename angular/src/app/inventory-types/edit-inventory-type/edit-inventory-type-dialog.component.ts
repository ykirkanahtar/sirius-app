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
  InventoryTypeDto,
  InventoryTypeServiceProxy,
  QuantityType,
} from "@shared/service-proxies/service-proxies";

@Component({
  templateUrl: "edit-inventory-type-dialog.component.html",
})
export class EditInventoryTypeDialogComponent
  extends AppComponentBase
  implements OnInit {
  saving = false;
  id: string;
  inventoryType = new InventoryTypeDto();
  quantityTypeEnum = QuantityType;
  quantityTypes: any[];

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _inventoryTypeService: InventoryTypeServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._inventoryTypeService
      .get(this.id)
      .subscribe((result: InventoryTypeDto) => {
        this.inventoryType = result;
      });

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

    const inventoryType = new InventoryTypeDto();
    inventoryType.init(this.inventoryType);

    this._inventoryTypeService
      .update(inventoryType)
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
