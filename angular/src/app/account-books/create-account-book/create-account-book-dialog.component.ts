import {
  Component,
  Injector,
  OnInit,
  EventEmitter,
  Output,
  Optional,
  Inject,
  ViewChild,
  Input,
} from "@angular/core";
import { finalize, throwIfEmpty } from "rxjs/operators";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import * as _ from "lodash";
import { AppComponentBase } from "@shared/app-component-base";
import { CommonFunctions } from "@shared/helpers/CommonFunctions";
import {
  AccountBookServiceProxy,
  HousingServiceProxy,
  PaymentAccountServiceProxy,
  LookUpDto,
  PaymentCategoryServiceProxy,
  CreateAccountBookDto,
  PersonServiceProxy,
  API_BASE_URL,
  PaymentCategoryDto,
  PaymentCategoryType,
  CreateInventoryDto,
  InventoryTypeServiceProxy,
  QuantityType,
  InventoryTypeDto,
} from "@shared/service-proxies/service-proxies";
import { HttpClient } from "@angular/common/http";
import * as moment from "moment";
import { CustomUploadServiceProxy } from "@shared/service-proxies/custom-service-proxies";
import { SelectItem } from "primeng/api";
import { CreateInventoryDialogComponent } from "@app/inventories/create-inventory/create-inventory-dialog.component";

@Component({
  templateUrl: "create-account-book-dialog.component.html",
})
export class CreateAccountBookDialogComponent
  extends AppComponentBase
  implements OnInit
{
  saving = false;
  saveLabel = this.l("Save");
  accountBook = new CreateAccountBookDto();

  housings: LookUpDto[];
  housingsForNetting: LookUpDto[];
  paymentAccounts: LookUpDto[];
  paymentCategories: LookUpDto[];
  paymentCategoriesForNetting: LookUpDto[];
  people: LookUpDto[];
  peopleForHousingDue: LookUpDto[];
  paymentCategory: PaymentCategoryDto;
  definedPaymentCategory: boolean = false;
  createInventory: CreateInventoryDto = new CreateInventoryDto();
  inventoryTypes: SelectItem[] = [];
  quantityTypes: SelectItem[] = [];

  paymentCategoryType?: PaymentCategoryType;
  PaymentCategoryTypeEnum = PaymentCategoryType;

  uploadedFileUrls: any[] = [];
  baseUrl: string;

  processDate: Date;
  documentDate: Date;

  @Input() lastAccountBookDate: moment.Moment;
  @Input() isHousingDue: boolean;

  @Output() onSave = new EventEmitter<any>();
  @ViewChild("fileUpload") fileUpload: any;

  constructor(
    injector: Injector,
    private _accountBookServiceProxy: AccountBookServiceProxy,
    private _housingServiceProxy: HousingServiceProxy,
    private _paymentAccountServiceProxy: PaymentAccountServiceProxy,
    private _paymentCategoryServiceProxy: PaymentCategoryServiceProxy,
    private _personServiceProxy: PersonServiceProxy,
    private _uploadServiceProxy: CustomUploadServiceProxy,
    private _inventoryTypeService: InventoryTypeServiceProxy,
    private _modalService: BsModalService,
    private http: HttpClient,
    public bsModalRef: BsModalRef,
    public inventoryModalRef: BsModalRef,
    @Optional() @Inject(API_BASE_URL) baseUrl?: string
  ) {
    super(injector);
    this.baseUrl = baseUrl ? baseUrl : "";
  }

  setDefaultPaymentAccount(paymentCategory: PaymentCategoryDto): void {
    if (paymentCategory.paymentCategoryType) {
      if (
        paymentCategory.paymentCategoryType === PaymentCategoryType.Income ||
        paymentCategory.paymentCategoryType ===
          PaymentCategoryType.TransferBetweenAccounts
      ) {
        this.accountBook.toPaymentAccountId =
          paymentCategory.defaultToPaymentAccountId;
      }

      if (
        paymentCategory.paymentCategoryType === PaymentCategoryType.Expense ||
        paymentCategory.paymentCategoryType ===
          PaymentCategoryType.TransferBetweenAccounts
      ) {
        this.accountBook.fromPaymentAccountId =
          paymentCategory.defaultFromPaymentAccountId;
      }
    }
  }

  ngOnInit(): void {
    if (this.paymentCategory) {
      this.accountBook.paymentCategoryId = this.paymentCategory.id;
      this.paymentCategoryType = this.paymentCategory.paymentCategoryType;
      this.isHousingDue = this.paymentCategory.isHousingDue;
      this.definedPaymentCategory = true;
      this.setDefaultPaymentAccount(this.paymentCategory);
      this.getHousings(this.paymentCategory);
    } else {
      this.getHousings();
    }

    this._paymentCategoryServiceProxy
      .getLookUpByPaymentCategoryType(true, this.paymentCategoryType)
      .subscribe((result: LookUpDto[]) => {
        this.paymentCategories = result;
        if (this.paymentCategories.length === 1) {
          this.accountBook.paymentCategoryId = this.paymentCategories[0].value;
          this.onSelectedPaymentCategoryChange(
            this.accountBook.paymentCategoryId
          );
        }
      });

    this.accountBook.isHousingDue = this.isHousingDue;

    this._paymentAccountServiceProxy
      .getPaymentAccountLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.paymentAccounts = result;
      });

    this._personServiceProxy
      .getPersonLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.people = result;
      });

    if (this.lastAccountBookDate) {
      this.processDate = this.lastAccountBookDate.toDate();
    }

    if (
      (this.accountBook.isHousingDue === undefined ||
        this.accountBook.isHousingDue === false) &&
      this.paymentCategoryType === PaymentCategoryType.Expense
    ) {
      this._inventoryTypeService
        .getLookUp()
        .subscribe((result: LookUpDto[]) => {
          this.inventoryTypes = result;

          this.inventoryTypes.forEach((item) => {
            this._inventoryTypeService
              .get(item.value)
              .subscribe((result: InventoryTypeDto) => {
                this.quantityTypes.push({
                  value: result.id,
                  label: result.quantityType.toString(),
                });
              });
          });
        });

      this.accountBook.inventories = [];
    }
  }

  isFromPaymentAccountRequired(): boolean {
    if (this.accountBook.nettingFromHousingDue) {
      return false;
    }

    return (
      this.paymentCategoryType === this.PaymentCategoryTypeEnum.Expense ||
      this.paymentCategoryType ===
        this.PaymentCategoryTypeEnum.TransferBetweenAccounts ||
      this.accountBook.nettingFromHousingDue === false
    );
  }

  onSelectedPersonChange(event) {
    var selectedPerson = event.value;

    if (!selectedPerson) {
      this.getHousings(this.paymentCategory);
    } else {
      this._housingServiceProxy
        .getHousingLookUp(selectedPerson, this.accountBook.paymentCategoryId)
        .subscribe((result: LookUpDto[]) => {
          this.housings = result;
          if (this.housings.length === 1) {
            this.accountBook.housingId = this.housings[0].value;
          }
        });
    }
  }

  isValidPaymentCategory(): boolean {
    return this.accountBook.paymentCategoryId !== undefined;
  }

  onSelectedPersonForNettingChange(event) {
    var selectedPerson = event.value;

    if (!selectedPerson) {
      this.getHousings();
    } else {
      this._housingServiceProxy
        .getHousingLookUp(selectedPerson, undefined)
        .subscribe((result: LookUpDto[]) => {
          this.housingsForNetting = result;
          if (this.housingsForNetting.length === 1) {
            this.accountBook.housingIdForNetting =
              this.housingsForNetting[0].value;
            this.onSelectedHousingForNettingChange(this.housingsForNetting);
          }
        });
    }
  }

  onSelectedHousingForNettingChange(event) {
    if (!this.accountBook.housingIdForNetting) {
      this.accountBook.paymentCategoryIdForNetting = null;
    } else {
      this._paymentCategoryServiceProxy
        .getLookUpByHousingId(this.accountBook.housingIdForNetting)
        .subscribe((result: LookUpDto[]) => {
          this.paymentCategoriesForNetting = result;
          if (this.housings.length === 1) {
            this.accountBook.paymentCategoryIdForNetting =
              this.paymentCategoriesForNetting[0].value;
          }
        });
    }
  }

  onSelectedInventoryTypeChange(event) {
    var selectedInventoryType = event.value;

    //Todo quantity type'ı çek
  }

  getPeopleForHousingDue(paymentCategoryId: string) {
    this._personServiceProxy
      .getPersonLookUpForHousingDue(this.accountBook.paymentCategoryId)
      .subscribe((result: LookUpDto[]) => {
        this.peopleForHousingDue = result;
      });
  }

  onSelectedPaymentCategoryChange(paymentCategoryId?: string) {
    if (paymentCategoryId) {
      this._paymentCategoryServiceProxy
        .get(paymentCategoryId)
        .subscribe((result: PaymentCategoryDto) => {
          this.setDefaultPaymentAccount(result);
          this.accountBook.isHousingDue = result.isHousingDue;
          this.getHousings(result);
          if (this.accountBook.isHousingDue === false) {
            this.accountBook.housingId = "00000000-0000-0000-0000-000000000000";
          }
        });

      this.getPeopleForHousingDue(paymentCategoryId);
    }
  }

  getHousings(paymentCategory?: PaymentCategoryDto) {
    if (paymentCategory != null) {
      this._housingServiceProxy
        .getHousingLookUp(undefined, paymentCategory.id)
        .subscribe((result: LookUpDto[]) => {
          this.housings = result;
          this.housingsForNetting = result;
          this.accountBook.housingIdForNetting = null;
        });
    } else {
      this._housingServiceProxy
        .getHousingLookUp(undefined, undefined)
        .subscribe((result: LookUpDto[]) => {
          this.housings = result;
          this.housingsForNetting = result;
          this.accountBook.housingIdForNetting = null;
        });
    }
  }

  uploadHandler(event): void {
    this.saving = true;
    this.saveLabel = this.l("File(s)IsUploading");

    if (event.files.length === 0) {
      console.log("No file selected.");
      return;
    }

    for (const file of event.files) {
      const input = new FormData();
      input.append("file", file);

      this._uploadServiceProxy.uploadFile(input).subscribe((result) => {
        this.uploadedFileUrls.push(result);
        this.fileUpload.clear();
        this.saving = false;
        this.saveLabel = this.l("Save");
      });
    }
  }

  addToInventories() {
    this.inventoryModalRef = this._modalService.show(
      CreateInventoryDialogComponent,
      {
        class: "modal-lg",
      }
    );

    this.inventoryModalRef.content.onlyAddToList = true;
    this.inventoryModalRef.content.event.subscribe((res) => {
      this.accountBook.inventories.push(res.data);
    });
  }

  getInventoryTypeName(inventoryTypeId: string): string {
    let label = "";
    this.inventoryTypes.forEach((item) => {
      if (item.value == inventoryTypeId) {
        label = item.label;
      }
    });
    return label;
  }

  getInventoryQuantityTypeName(inventoryTypeId: string): string {
    let label = "";
    this.quantityTypes.forEach((item) => {
      if (item.value == inventoryTypeId) {
        label = item.label;
      }
    });
    return QuantityType[label];
  }

  removeInventory(inventory: CreateInventoryDto) {
    abp.message.confirm(
      this.l("DeleteWarningMessage"),
      undefined,
      (result: boolean) => {
        if (result) {
          const index = this.accountBook.inventories.indexOf(
            inventory,
            0
          );
          if (index > -1) {
            this.accountBook.inventories.splice(index, 1);
          }
        }
      }
    );
  }

  save(): void {
    this.saving = true;
    this.saveLabel = this.l("Processing");

    this.accountBook.accountBookFileUrls = [];

    this.accountBook.processDateString = CommonFunctions.dateToString(
      this.processDate
    );

    if (this.documentDate !== undefined && this.documentDate) {
      this.accountBook.documentDateTimeString = CommonFunctions.dateToString(
        this.documentDate
      );
    }

    for (const fileUrl of this.uploadedFileUrls) {
      this.accountBook.accountBookFileUrls.push(fileUrl);
    }

    this._accountBookServiceProxy
      .create(this.accountBook)
      .pipe(
        finalize(() => {
          this.saving = false;
          this.saveLabel = this.l("Save");
        })
      )
      .subscribe(() => {
        this.notify.info(this.l("SavedSuccessfully"));
        this.bsModalRef.hide();
        this.onSave.emit();
      });
  }
}
