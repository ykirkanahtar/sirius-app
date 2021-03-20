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
import { finalize } from "rxjs/operators";
import { BsModalRef } from "ngx-bootstrap/modal";
import * as _ from "lodash";
import { AppComponentBase } from "@shared/app-component-base";
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
} from "@shared/service-proxies/service-proxies";
import { HttpClient } from "@angular/common/http";
import * as moment from "moment";
import { CommonFunctions } from "@shared/helpers/CommonFunctions";
import { CustomUploadServiceProxy } from "@shared/service-proxies/custom-service-proxies";
import { threadId } from "worker_threads";

@Component({
  templateUrl: "create-account-book-dialog.component.html",
})
export class CreateAccountBookDialogComponent
  extends AppComponentBase
  implements OnInit {
  saving = false;
  saveLabel = this.l("Save");
  accountBook = new CreateAccountBookDto();

  housings: LookUpDto[];
  paymentAccounts: LookUpDto[];
  paymentCategories: LookUpDto[];
  people: LookUpDto[];
  paymentCategory: PaymentCategoryDto;
  definedPaymentCategory: boolean = false;

  paymentCategoryType?: PaymentCategoryType;
  PaymentCategoryTypeEnum = PaymentCategoryType;

  uploadedFileUrls: any[] = [];
  baseUrl: string;

  processDate: Date;

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
    private http: HttpClient,
    public bsModalRef: BsModalRef,
    @Optional() @Inject(API_BASE_URL) baseUrl?: string
  ) {
    super(injector);
    this.baseUrl = baseUrl ? baseUrl : "";
  }

  setDefaultPaymentAccount(paymentCategory: PaymentCategoryDto): void {
    if(paymentCategory.paymentCategoryType) {
      if(paymentCategory.paymentCategoryType === PaymentCategoryType.Income || 
        paymentCategory.paymentCategoryType === PaymentCategoryType.TransferBetweenAccounts) {
          this.accountBook.toPaymentAccountId = paymentCategory.defaultToPaymentAccountId;
        }

        if(paymentCategory.paymentCategoryType === PaymentCategoryType.Expense || 
          paymentCategory.paymentCategoryType === PaymentCategoryType.TransferBetweenAccounts) {
            this.accountBook.toPaymentAccountId = paymentCategory.defaultFromPaymentAccountId;
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
    }

    this._paymentCategoryServiceProxy
      .getLookUpByPaymentCategoryType(true, this.paymentCategoryType)
      .subscribe((result: LookUpDto[]) => {
        this.paymentCategories = result;
      });

    this.accountBook.isHousingDue = this.isHousingDue;

    this.getHousings();

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

    this.accountBook.processDateTime = this.lastAccountBookDate;

    if (this.accountBook.processDateTime) {
      this.processDate = this.accountBook.processDateTime.toDate();
    }
  }

  onSelectedPersonChange(event) {
    var selectedPerson = event.value;

    if (!selectedPerson) {
      this.getHousings();
    } else {
      this._housingServiceProxy
        .getHousingsLookUpByPersonId(selectedPerson)
        .subscribe((result: LookUpDto[]) => {
          this.housings = result;
          if (this.housings.length === 1) {
            this.accountBook.housingId = this.housings[0].value;
          }
        });
    }
  }

  onSelectedPaymentCategoryChange(event) {
    var selectedPaymentCategory = event.value;

    if (selectedPaymentCategory) {
      this._paymentCategoryServiceProxy
        .get(selectedPaymentCategory)
        .subscribe((result: PaymentCategoryDto) => {
          // this.accountBook.fromPaymentAccountId =
          //   result.defaultFromPaymentAccountId;
          // this.accountBook.toPaymentAccountId =
          //   result.defaultToPaymentAccountId;
          this.setDefaultPaymentAccount(selectedPaymentCategory);
          this.accountBook.isHousingDue = result.isHousingDue;
          if(this.accountBook.isHousingDue === false) {
            this.accountBook.housingId = "00000000-0000-0000-0000-000000000000";
          }
        });
    }
  }

  getHousings() {
    this._housingServiceProxy
      .getHousingLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.housings = result;
        this.accountBook.housingIdForEncachment = null;
      });
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

  save(): void {
    this.saving = true;
    this.saveLabel = this.l("Processing");

    this.accountBook.accountBookFileUrls = [];
    this.accountBook.processDateTime = CommonFunctions.toMoment(
      this.processDate
    );

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
