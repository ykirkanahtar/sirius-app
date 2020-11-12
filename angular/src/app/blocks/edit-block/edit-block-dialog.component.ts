import {
  Component,
  Injector,
  OnInit,
  EventEmitter,
  Output,
} from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BsModalRef } from 'ngx-bootstrap/modal';
import * as _ from 'lodash';
import { AppComponentBase } from '@shared/app-component-base';
import { BlockDto, BlockServiceProxy } from '@shared/service-proxies/service-proxies';


@Component({
  templateUrl: 'edit-block-dialog.component.html'
})
export class EditBlockDialogComponent extends AppComponentBase
  implements OnInit {
  saving = false;
  id: string;
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
    this._blockService
      .get(this.id)
      .subscribe((result: BlockDto) => {
        this.block = result;
      });
  }

  save(): void {
    this.saving = true;

    const block = new BlockDto();
    block.init(this.block);

    this._blockService
      .update(block)
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
