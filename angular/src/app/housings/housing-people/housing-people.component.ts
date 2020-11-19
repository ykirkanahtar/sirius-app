import {
  Component,
  Injector,
  EventEmitter,
  Output,
  ViewChild,
  OnInit,
} from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef } from 'ngx-bootstrap/modal';
import * as _ from 'lodash';
import {
  BlockDto,
  HousingDto,
  HousingPaymentPlanDto,
  HousingPaymentPlanDtoPagedResultDto,
  HousingPaymentPlanServiceProxy,
  HousingPersonDto,
  HousingPersonDtoPagedResultDto,
  HousingServiceProxy,
  PaymentPlanType
} from '@shared/service-proxies/service-proxies';
import {
  PagedListingComponentBase,
  PagedRequestDto
} from '@shared/paged-listing-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/table';
import { LazyLoadEvent } from 'primeng/api';

class PagedHousingPaymentPlanResultRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: './housing-people.component.html',
  animations: [appModuleAnimation()]
})
export class HousingPeopleDialogComponent extends PagedListingComponentBase<HousingPersonDto>
  implements OnInit {

  @ViewChild('dataTable', { static: true }) dataTable: Table;

  sortingColumn: string;

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

    this._housingService
      .get(this.id)
      .subscribe((result: HousingDto) => {
        this.housing = result;
        this.block = this.housing.block;
      });

    this.getDataPage(1);
  }

  getData(event?: LazyLoadEvent) {
    this.sortingColumn = this.primengTableHelper.getSorting(this.dataTable);
    this.getDataPage(1);
  }

  protected list(
    request: PagedHousingPaymentPlanResultRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {

    this._housingService
      .getHousingPeople(
        this.id,
        this.sortingColumn,
        request.skipCount,
        request.maxResultCount
      )
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: HousingPersonDtoPagedResultDto) => {
        this.housingPeople = result.items;
        this.showPaging(result, pageNumber);
      });
  }

  protected delete(housingPerson: HousingPersonDto): void {
    abp.message.confirm(
      this.l(
        'HousingPersonDeleteWarningMessage',
        housingPerson.person.firstName + ' ' + housingPerson.person.lastName
      ),
      undefined,
      (result: boolean) => {
        if (result) {
          this._housingService.removePerson(housingPerson.housingId, housingPerson.personId).subscribe(() => {
            abp.notify.success(this.l('SuccessfullyDeleted'));
            this.refresh();
          });
        }
      }
    );
  }
}
