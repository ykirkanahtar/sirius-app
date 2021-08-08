import {
  Component,
  Injector,
  EventEmitter,
  Input,
  Output,
  OnInit,
} from "@angular/core";
import { finalize } from "rxjs/operators";
import { BsModalRef } from "ngx-bootstrap/modal";
import * as _ from "lodash";
import { AppComponentBase } from "@shared/app-component-base";
import {
  CreateInventoryDto,
  InventoryServiceProxy,
  InventoryTypeDto,
  InventoryTypeServiceProxy,
  LookUpDto,
  QuantityType,
} from "@shared/service-proxies/service-proxies";
import { SelectItem } from "primeng/api";

@Component({
  templateUrl: "create-inventory-dialog.component.html",
})
export class CreateInventoryDialogComponent
  extends AppComponentBase
  implements OnInit
{
  saving = false;
  input = new CreateInventoryDto();
  buttonText = "selam";

  quantityTypeEnum = QuantityType;
  inventoryTypes: SelectItem[] = [];

  @Input() onlyAddToList: boolean;
  @Output() onSave = new EventEmitter<any>();
  public event: EventEmitter<any> = new EventEmitter<any>();

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

  addToList() {
    this._inventoryTypeService
      .get(this.input.inventoryTypeId)
      .subscribe((result: InventoryTypeDto) => {
        this.event.emit({ data: this.input, inventoryType: result, res: 200 });
        this.bsModalRef.hide();
      });
  }

  save(): void {
    this.saving = true;

    this._inventoryService
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
