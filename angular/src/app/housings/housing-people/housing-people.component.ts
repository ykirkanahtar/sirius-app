import {
  Component,
  Injector,
  EventEmitter,
  Output,
  ViewChild,
  OnInit,
} from "@angular/core";
import { BsModalRef } from "ngx-bootstrap/modal";
import * as _ from "lodash";
import {
  BlockDto,
  HousingDto,
  HousingPersonDto,
  HousingPersonDtoPagedResultDto,
  HousingServiceProxy,
} from "@shared/service-proxies/service-proxies";
import { PagedRequestDto } from "@shared/paged-listing-component-base";
import { appModuleAnimation } from "@shared/animations/routerTransition";
import { Table } from "primeng/table";
import { LazyLoadEvent } from "primeng/api";
import { AppComponentBase } from "@shared/app-component-base";

class PagedHousingPaymentPlanResultRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: "./housing-people.component.html",
  animations: [appModuleAnimation()],
})
export class HousingPeopleDialogComponent
  extends AppComponentBase
  implements OnInit {
  @ViewChild("dataTable", { static: true }) dataTable: Table;

  sortingColumn: string;
  skipCount: number;
  maxResultCount: number;
  totalRecords: number;
  isTableLoading: boolean = false;

  housingPeople: HousingPersonDto[] = [];
  housing = new HousingDto();
  block = new BlockDto();

  id: string;
  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _housingService: HousingServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this._housingService.get(this.id).subscribe((result: HousingDto) => {
      this.housing = result;
      this.block = this.housing.block;
    });
  }

  getData(event?: LazyLoadEvent) {
    this.isTableLoading = true;

    this.sortingColumn = this.primengTableHelper.getSorting(this.dataTable);

    if (event.first) {
      this.skipCount = event.first;
    }

    if (event.first || event.rows) {
      this.maxResultCount = event.first + event.rows;
    }

    this._housingService
      .getHousingPeople(
        this.id,
        this.sortingColumn,
        this.skipCount,
        this.maxResultCount
      )
      .subscribe((result: HousingPersonDtoPagedResultDto) => {
        this.housingPeople = result.items;
        this.totalRecords = result.totalCount;
        this.isTableLoading = false;
      });
  }

  protected delete(
    housingPerson: HousingPersonDto,
    event?: LazyLoadEvent
  ): void {
    abp.message.confirm(
      this.l(
        "HousingPersonDeleteWarningMessage",
        housingPerson.person.firstName + " " + housingPerson.person.lastName
      ),
      undefined,
      (result: boolean) => {
        if (result) {
          this._housingService
            .removePerson(housingPerson.housingId, housingPerson.personId)
            .subscribe(() => {
              abp.notify.success(this.l("SuccessfullyDeleted"));
              this.getData(event);
            });
        }
      }
    );
  }
}
