import { Component, Injector, OnInit } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {
    FinancialStatementDto,
    ReportServiceProxy,
} from '@shared/service-proxies/service-proxies';
import { LazyLoadEvent } from 'primeng/api';
import {AppComponentBase} from '../../shared/app-component-base';

@Component({
    templateUrl: './financial-statements.component.html',
    styleUrls: ['./financial-statements.component.less'],
    animations: [appModuleAnimation()],
})
export class FinancialStatementsComponent
    extends AppComponentBase
    implements OnInit {

    content: FinancialStatementDto;

    constructor(
        injector: Injector,
        private _reportService: ReportServiceProxy
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this.getData();
    }

    public onReady( editor ) {
        editor.ui.getEditableElement().parentElement.insertBefore(
            editor.ui.view.toolbar.element,
            editor.ui.getEditableElement()
        );
    }

    getData(event?: LazyLoadEvent) {
        this._reportService
            .getFinancialStatement()
            .subscribe((result: FinancialStatementDto) => {
                this.content = result;
            });
    }
}
