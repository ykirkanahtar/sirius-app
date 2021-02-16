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
  import { PeriodDto, PeriodServiceProxy, CreatePeriodForSiteDto, CreatePeriodForBlockDto, PeriodFor, BlockServiceProxy, LookUpDto } from '@shared/service-proxies/service-proxies';
  
  @Component({
    templateUrl: 'create-period-dialog.component.html'
  })
  export class CreatePeriodDialogComponent extends AppComponentBase
    implements OnInit {
    saving = false;
    period = new PeriodDto();
    periodFor: PeriodFor;
    periodForEnum = PeriodFor;
    blockItems = [];
    selectedBlock: string;
  
    @Output() onSave = new EventEmitter<any>();
  
    constructor(
      injector: Injector,
      private _periodService: PeriodServiceProxy,
      private _blockService: BlockServiceProxy,
      public bsModalRef: BsModalRef
    ) {
      super(injector);
    }
  
    ngOnInit(): void {
      this._blockService
      .getBlockLookUp()
      .subscribe((result: LookUpDto[]) => {
        this.blockItems = result;
        console.log(this.blockItems);
      });  
    }
  
    save(): void {
      this.saving = true;

      if(this.periodFor === PeriodFor.Site) {
        const period = new CreatePeriodForSiteDto();
        period.init(this.period);
    
        this._periodService
          .createForSite(period)
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
      } else {
        const period = new CreatePeriodForBlockDto();
        period.init(this.period);
        this.period.blockId = this.selectedBlock;
    
        this._periodService
          .createForBlock(period)
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
  }
  