import {
Component,
Injector,
OnInit,
  EventEmitter,
  Output,
  Input
} from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef } from 'ngx-bootstrap/modal';
import * as _ from 'lodash';
import { AppComponentBase } from '@shared/app-component-base';
import { AccountBookDto, AccountBookServiceProxy } from '@shared/service-proxies/service-proxies';

@Component({
  templateUrl: 'create-housing-due-account-book-dialog.component.html'
})
export class CreateHousingDueAccountBookDialogComponent extends AppComponentBase
  implements OnInit {
  saving = false;
  accountBook = new AccountBookDto();

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _accountBookServiceProxy: AccountBookServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {

  }

  save(): void {
    this.saving = true;

   
  }
}
