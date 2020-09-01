import { Component, Injector } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {
  PagedListingComponentBase,
  PagedRequestDto
} from '@shared/paged-listing-component-base';
import { CreatePersonDialogComponent } from './create-person/create-person-dialog.component';
import { EditPersonDialogComponent } from './edit-person/edit-person-dialog.component';
import { PersonDto, PersonServiceProxy, PersonDtoPagedResultDto } from '@shared/service-proxies/service-proxies';

class PagedPeopleRequestDto extends PagedRequestDto {
  keyword: string;
}

@Component({
  templateUrl: './people.component.html',
  animations: [appModuleAnimation()]
})
export class PeopleComponent extends PagedListingComponentBase<PersonDto> {
  people: PersonDto[] = [];
  keyword = '';

  constructor(
    injector: Injector,
    private _peopleService: PersonServiceProxy,
    private _modalService: BsModalService
  ) {
    super(injector);
  }

  list(
    request: PagedPeopleRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.keyword = this.keyword;

    this._peopleService
      .getAll(request.keyword, true, request.skipCount, request.maxResultCount)
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: PersonDtoPagedResultDto) => {
        this.people = result.items;
        this.showPaging(result, pageNumber);
      });
  }

  delete(person: PersonDto): void {
    abp.message.confirm(
      this.l('PersonDeleteWarningMessage', person.firstName + ' ' + person.lastName),
      undefined,
      (result: boolean) => {
        if (result) {
          this._peopleService
            .delete(person.id)
            .pipe(
              finalize(() => {
                abp.notify.success(this.l('SuccessfullyDeleted'));
                this.refresh();
              })
            )
            .subscribe(() => {});
        }
      }
    );
  }

  createPerson(): void {
    this.showCreateOrEditPersonDialog();
  }

  editPerson(person: PersonDto): void {
    this.showCreateOrEditPersonDialog(person.id);
  }

  showCreateOrEditPersonDialog(id?: string): void {
    let createOrEditPersonDialog: BsModalRef;
    if (!id) {
      createOrEditPersonDialog = this._modalService.show(
        CreatePersonDialogComponent,
        {
          class: 'modal-lg',
        }
      );
    } else {
      createOrEditPersonDialog = this._modalService.show(
        EditPersonDialogComponent,
        {
          class: 'modal-lg',
          initialState: {
            id: id,
          },
        }
      );
    }

    createOrEditPersonDialog.content.onSave.subscribe(() => {
      this.refresh();
    });
  }
}
