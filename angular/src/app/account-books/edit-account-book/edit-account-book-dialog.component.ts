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
  PersonServiceProxy,
  API_BASE_URL,
  UpdateAccountBookDto,
  AccountBookDto,
  PaymentAccountDto,
  HousingDto,
} from "@shared/service-proxies/service-proxies";
import { HttpClient } from "@angular/common/http";
import * as moment from "moment";
import { CommonFunctions } from "@shared/helpers/CommonFunctions";
import { update } from "lodash";

@Component({
  templateUrl: "edit-account-book-dialog.component.html",
})
export class EditAccountBookDialogComponent
  extends AppComponentBase
  implements OnInit {
  saving = false;
  saveLabel = this.l("Save");
  id: string;

  accountBook = new AccountBookDto();

  housing = new HousingDto();
  paymentAccounts: LookUpDto[];
  paymentCategories: LookUpDto[];
  people: LookUpDto[];

  uploadedFileUrls: any[] = [];
  newUploadedFileUrls: any[] = [];
  deletedFileUrls: any[] = [];
  baseUrl: string;

  processDate: Date;

  @Input() lastAccountBookDate: moment.Moment;

  @Output() onSave = new EventEmitter<any>();
  @ViewChild("fileUpload") fileUpload: any;

  constructor(
    injector: Injector,
    private _accountBookServiceProxy: AccountBookServiceProxy,
    private _paymentAccountServiceProxy: PaymentAccountServiceProxy,
    private _paymentCategoryServiceProxy: PaymentCategoryServiceProxy,
    private _personServiceProxy: PersonServiceProxy,
    private http: HttpClient,
    public bsModalRef: BsModalRef,
    @Optional() @Inject(API_BASE_URL) baseUrl?: string
  ) {
    super(injector);
    this.baseUrl = baseUrl ? baseUrl : "";
  }

  ngOnInit(): void {
    this._accountBookServiceProxy
      .get(this.id)
      .subscribe((result: AccountBookDto) => {
        this.accountBook = result;
        this.processDate = this.accountBook.processDateTime.toDate();
      });

    this._paymentAccountServiceProxy
      .getPaymentAccountLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.paymentAccounts = result;
      });

    this._paymentCategoryServiceProxy
      .getPaymentCategoryLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.paymentCategories = result;
      });

    this._personServiceProxy
      .getPersonLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.people = result;
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
      this.http.post(this.baseUrl + "/Upload/Upload", input).subscribe(
        (data) => {
          this.uploadedFileUrls.push(data["result"]);
          this.saving = false;
          this.saveLabel = this.l("Save");
        },
        (err) => {
          abp.message.error(err.statusText);
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

    this.accountBook.processDateTime = CommonFunctions.toMoment(
      this.processDate
    );

    const updateAccountBookDto = new UpdateAccountBookDto();
    updateAccountBookDto.init(this.accountBook);

    updateAccountBookDto.newAccountBookFileUrls = [];

    for (const fileUrl of this.newUploadedFileUrls) {
      updateAccountBookDto.newAccountBookFileUrls.push(fileUrl);
    }

    this._accountBookServiceProxy
      .update(updateAccountBookDto)
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
