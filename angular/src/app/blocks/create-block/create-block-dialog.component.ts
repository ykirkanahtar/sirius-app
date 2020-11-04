import {
  Component,
  Injector,
  OnInit,
  EventEmitter,
  Output
} from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef } from 'ngx-bootstrap/modal';
import * as _ from 'lodash';
import { AppComponentBase } from '@shared/app-component-base';
import { BlockDto, BlockServiceProxy, CreateBlockDto } from '@shared/service-proxies/service-proxies';

@Component({
  templateUrl: 'create-block-dialog.component.html'
})
export class CreateBlockDialogComponent extends AppComponentBase
  implements OnInit {
  saving = false;
  block = new BlockDto();

  @Output() onSave = new EventEmitter<any>();

  constructor(
    injector: Injector,
    private _blockService: BlockServiceProxy,
    public bsModalRef: BsModalRef
  ) {
    super(injector);
  }

  ngOnInit(): void {

  }

  save(): void {
    this.saving = true;

    const block = new CreateBlockDto();
    block.init(this.block);

    this._blockService
      .create(block)
      .pipe(
        finalize(() => {
          this.saving = false;
        })
      )
      .subscribe(() => {
        this.notify.info(this.l('SavedSuccessfully'));
        this.bsModalRef.hide();
        this.onSave.emit();
      });
  }
}
