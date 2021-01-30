import {
  Component,
  Injector,
  OnInit,
  EventEmitter,
  Output,
  Inject,
  Optional,
  ViewChild,
  Input,
} from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { finalize, last } from "rxjs/operators";
import { BsModalRef } from "ngx-bootstrap/modal";
import * as _ from "lodash";
import { AppComponentBase } from "@shared/app-component-base";
import {
  AccountBookDto,
  AccountBookServiceProxy,
  HousingServiceProxy,
  PaymentAccountServiceProxy,
  LookUpDto,
  CreateHousingDueAccountBookDto,
  PersonServiceProxy,
} from "@shared/service-proxies/service-proxies";
import { API_BASE_URL } from "@shared/service-proxies/service-proxies";
import { throwError as _observableThrow, of as _observableOf } from "rxjs";
import * as moment from "moment";
import { CommonFunctions } from '@shared/helpers/CommonFunctions';
import { CustomUploadServiceProxy } from "@shared/service-proxies/custom-service-proxies";

@Component({
  templateUrl: "create-housing-due-account-book-dialog.component.html",
})
export class CreateHousingDueAccountBookDialogComponent
  extends AppComponentBase
  implements OnInit {
  saving = false;
  saveLabel = this.l("Save");

  accountBook = new AccountBookDto();

  housings: LookUpDto[];
  paymentAccounts: LookUpDto[];
  people: LookUpDto[];

  uploadedFileUrls: any[] = [];
  baseUrl: string;
  
  processDate: Date;

  @Input() lastAccountBookDate: moment.Moment;

  @Output() onSave = new EventEmitter<any>();
  @ViewChild("fileUpload") fileUpload: any;

  constructor(
    injector: Injector,
    private _accountBookServiceProxy: AccountBookServiceProxy,
    private _housingServiceProxy: HousingServiceProxy,
    private _paymentAccountServiceProxy: PaymentAccountServiceProxy,
    private _personServiceProxy: PersonServiceProxy,
    private _uploadServiceProxy: CustomUploadServiceProxy,
    private http: HttpClient,
    public bsModalRef: BsModalRef,
    @Optional() @Inject(API_BASE_URL) baseUrl?: string
  ) {
    super(injector);
    this.baseUrl = baseUrl ? baseUrl : "";    
  }

  ngOnInit(): void {
    this.getHousings();

    this._paymentAccountServiceProxy
      .getPaymentAccountLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.paymentAccounts = result;
      });

    this._paymentAccountServiceProxy
      .getDefaultPaymentAccount()
      .subscribe((result) => {
        this.accountBook.toPaymentAccountId = result.id;
      });

    this._personServiceProxy
      .getPersonLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.people = result;
      });

      this.accountBook.processDateTime = this.lastAccountBookDate;

      if(this.accountBook.processDateTime) {
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

  getHousings() {
    this._housingServiceProxy
      .getHousingLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.housings = result;
        this.accountBook.housingId = null;
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

      this._uploadServiceProxy
        .uploadFile(input)
        .subscribe(
          (result) => {
            this.uploadedFileUrls.push(result);
            this.fileUpload.clear();
            this.saving = false;
            this.saveLabel = this.l("Save");
          }
        );
    }
  }

  save(): void {
    this.saving = true;
    this.saveLabel = this.l("Processing");

    const accountBook = new CreateHousingDueAccountBookDto();
    accountBook.init(this.accountBook);
    accountBook.processDateTime = CommonFunctions.toMoment(this.processDate);

    accountBook.accountBookFileUrls = [];

    for (const fileUrl of this.uploadedFileUrls) {
      accountBook.accountBookFileUrls.push(fileUrl);
    }

    this._accountBookServiceProxy
      .createHousingDue(accountBook)
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
