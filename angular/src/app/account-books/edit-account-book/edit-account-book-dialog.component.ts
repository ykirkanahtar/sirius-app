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
  PaymentAccountServiceProxy,
  LookUpDto,
  PaymentCategoryServiceProxy,
  PersonServiceProxy,
  API_BASE_URL,
  UpdateAccountBookDto,
  AccountBookDto,
  HousingDto,
  AccountBookFileDto,
  HousingServiceProxy,
  PaymentCategoryLookUpDto,
} from "@shared/service-proxies/service-proxies";
import { HttpClient } from "@angular/common/http";
import * as moment from "moment";
import { CommonFunctions } from "@shared/helpers/CommonFunctions";
import { update } from "lodash";
import { CustomUploadServiceProxy } from "@shared/service-proxies/custom-service-proxies";
import { Observable } from "rxjs";

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
  paymentCategories: PaymentCategoryLookUpDto[] = [];
  people: LookUpDto[];

  newUploadedFileUrls: any[] = [];
  deletedUploadedFileUrls: any[] = [];
  deletedFileUrls: any[] = [];
  display: boolean = false;
  clickedImages: string[] = [];
  showPaymentCategory = false;

  processDate: Date;
  encashmentHousingText: string;

  @Input() lastAccountBookDate: moment.Moment;

  @Output() onSave = new EventEmitter<any>();
  @ViewChild("fileUpload") fileUpload: any;

  responsiveOptions2: any[] = [
    {
      breakpoint: "1500px",
      numVisible: 5,
    },
    {
      breakpoint: "1024px",
      numVisible: 3,
    },
    {
      breakpoint: "768px",
      numVisible: 2,
    },
    {
      breakpoint: "560px",
      numVisible: 1,
    },
  ];

  constructor(
    injector: Injector,
    private _accountBookServiceProxy: AccountBookServiceProxy,
    private _paymentAccountServiceProxy: PaymentAccountServiceProxy,
    private _paymentCategoryServiceProxy: PaymentCategoryServiceProxy,
    private _personServiceProxy: PersonServiceProxy,
    private _uploadServiceProxy: CustomUploadServiceProxy,
    private _housingServiceProxy: HousingServiceProxy,
    private http: HttpClient,
    public bsModalRef: BsModalRef,
    @Optional() @Inject(API_BASE_URL) baseUrl?: string
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._accountBookServiceProxy
      .get(this.id)
      .subscribe((result: AccountBookDto) => {
        this.accountBook = result;
        this.processDate = this.accountBook.processDateTime.toDate();

        if (this.accountBook.encashmentHousing) {
          this._housingServiceProxy
            .get(this.accountBook.housingIdForEncachment)
            .subscribe((result: HousingDto) => {
              this.encashmentHousingText =
                result.block.blockName + " " + result.apartment;
            });
        }
      });

    this._paymentAccountServiceProxy
      .getPaymentAccountLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.paymentAccounts = result;
      });

    this._accountBookServiceProxy
      .getPaymentCategoryLookUpForEditAccountBook(this.id)
      .subscribe((result: PaymentCategoryLookUpDto[]) => {
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

      this._uploadServiceProxy.uploadFile(input).subscribe((result) => {
        this.newUploadedFileUrls.push(result);
        this.fileUpload.clear();
        this.saving = false;
        this.saveLabel = this.l("Save");
      });
    }
  }

  canEditPaymentCategory(paymentCategoryId: string): boolean {
    if (paymentCategoryId) {
      //sayfa yüklenene kadar paymentCategoryId undefined geldiği için bu kontrol eklendi, o zamana kadar da component disabled yapılıyor

      let editInAccountBooks = this.paymentCategories
        .filter(
          (paymentCategory) => paymentCategory.value === paymentCategoryId
        )
        .map((p) => p.editInAccountBook);

      return editInAccountBooks[0];
    }

    return false;
  }

  showImages(clickedImages: string) {
    this.display = true;
    this.clickedImages.push(clickedImages);
  }

  delete(accountBookFile: AccountBookFileDto) {
    abp.message.confirm(
      this.l("AccountBookFileDeleteWarningMessage"),
      undefined,
      (result: boolean) => {
        if (result) {
          const index: number = this.accountBook.accountBookFiles.indexOf(
            accountBookFile
          );
          if (index !== -1) {
            this.accountBook.accountBookFiles.splice(index, 1);
            this.deletedFileUrls.push(accountBookFile.fileUrl);
          }
        }
      }
    );
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

    updateAccountBookDto.deletedAccountBookFileUrls = [];

    for (const fileUrl of this.deletedFileUrls) {
      updateAccountBookDto.deletedAccountBookFileUrls.push(fileUrl);
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
