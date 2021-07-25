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
  InventoryDto,
  InventoryServiceProxy,
  InventoryTypeDto,
  InventoryTypeServiceProxy,
  LookUpDto,
  QuantityType,
} from "@shared/service-proxies/service-proxies";
import { SelectItem } from "primeng/api";

@Component({
  templateUrl: "edit-inventory-dialog.component.html",
})
export class EditInventoryDialogComponent
  extends AppComponentBase
  implements OnInit {
  saving = false;
  id: string;
  inventory = new InventoryDto();
  inventoryTypes: SelectItem[] = [];

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _inventoryService: InventoryServiceProxy,
    private _inventoryTypeService: InventoryTypeServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._inventoryTypeService.getLookUp().subscribe((result: LookUpDto[]) => {
      this.inventoryTypes = result;
    });
  }

  onSelectedInventoryTypeChange(event) {
    var selectedInventoryType = event.value;

    //Todo quantity type'ı çek
  }

  save(): void {
    this.saving = true;

    const inventory = new InventoryDto();
    inventory.init(this.inventory);

    this._inventoryService
      .update(inventory)
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
