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
  PaymentCategoryDto,
  PaymentCategoryType,
} from "@shared/service-proxies/service-proxies";
import { HttpClient } from "@angular/common/http";
import * as moment from "moment";
import { CustomUploadServiceProxy } from "@shared/service-proxies/custom-service-proxies";
import { CommonFunctions } from "@shared/helpers/CommonFunctions";

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
  paymentCategory = new PaymentCategoryDto();
  PaymentCategoryTypeEnum = PaymentCategoryType;

  paymentAccounts: LookUpDto[];
  paymentCategories: LookUpDto[] = [];
  people: LookUpDto[];

  newUploadedFileUrls: any[] = [];
  deletedUploadedFileUrls: any[] = [];
  deletedFileUrls: any[] = [];
  display: boolean = false;
  clickedImages: string[] = [];
  showPaymentCategory = false;

  processDate: Date;
  documentDate: Date;
  housingDueBlockAndApartmentText: string;
  nettingHousingText: string;

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

        if (this.accountBook.documentDateTime) {
          this.documentDate = this.accountBook.documentDateTime.toDate();
        }

        this._paymentCategoryServiceProxy
          .get(this.accountBook.paymentCategoryId)
          .subscribe((paymentCategory: PaymentCategoryDto) => {
            this.paymentCategory = paymentCategory;

            if (paymentCategory.isHousingDue) {
              this._housingServiceProxy
                .get(this.accountBook.housingId)
                .subscribe((result: HousingDto) => {
                  this.housingDueBlockAndApartmentText =
                    result.block.blockName + " " + result.apartment;
                });
            }

            this._accountBookServiceProxy
              .getPaymentCategoryLookUpForEditAccountBook(this.id)
              .subscribe((result: LookUpDto[]) => {
                this.paymentCategories = result;
              });
          });

        if (this.accountBook.nettingHousing) {
          this._housingServiceProxy
            .get(this.accountBook.housingIdForNetting)
            .subscribe((result: HousingDto) => {
              this.nettingHousingText =
                result.block.blockName + " " + result.apartment;
            });
        }
      });

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

    const updateAccountBookDto = new UpdateAccountBookDto();
    updateAccountBookDto.init(this.accountBook);

    updateAccountBookDto.processDateString = CommonFunctions.dateToString(
      this.processDate
    );

    if (this.documentDate !== undefined && this.documentDate) {
      updateAccountBookDto.documentDateTimeString = CommonFunctions.dateToString(
        this.documentDate
      );
    }

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
