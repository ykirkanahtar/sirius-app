import { Component, Injector, OnInit } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import {
    FinancialStatementDto,
    ReportServiceProxy,
} from '@shared/service-proxies/service-proxies';
import { LazyLoadEvent } from 'primeng/api';
import {AppComponentBase} from '../../shared/app-component-base';
import { ToolbarModule } from '@syncfusion/ej2-angular-navigations';

@Component({
    templateUrl: './financial-statements.component.html',
    styleUrls: ['./financial-statements.component.less'],
    animations: [appModuleAnimation()],
})
export class FinancialStatementsComponent
    extends AppComponentBase
    implements OnInit {

    constructor(
        injector: Injector,
        private _reportService: ReportServiceProxy
    ) {
        super(injector);
    }

    value = '';

    public tools: ToolbarModule = {
        items: ['Bold', 'Italic', 'Underline', '|', 'Alignments',
            'OrderedList', 'UnorderedList', '|', 'Undo', 'Redo', 'Print']
    };

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
                let titles = '';

                let reportCenterTitleStyle = "text-align: center;font-weight: bold;";
                let listItemStyle = "margin-left: 5%;";
                let reportFloatRightTextStyle = "margin-right: 5%;float: right;";
                let boldTextStyle = "font-weight: bold;";
                let reportCenterTextStyle = "margin: 0 auto;width: 400px;";
                let reportLeftTextStyle = "margin-left: 5%;text-align: left;";

                result.title.forEach(function (title) {  
                    titles = titles + "<div style='" + reportCenterTitleStyle + "'>" + title + "</div>"; 
                });  

                let incomes : string = '';
                result.incomes.forEach(function (income) {  
                    incomes = incomes + `                    
                    <li style="` + listItemStyle + `">
                        ` + income.description +`
                        <span style="` + reportFloatRightTextStyle + `;` + boldTextStyle + `">` + income.amount + `</span>
                    </li>`
                });  

                let incomeTotal : string = '';
                if(result.incomes.length > 0) {
                    incomeTotal += ` 
                    <div style="` + reportFloatRightTextStyle + `;` + boldTextStyle +`">` + result.incomeTotal.amount + `</div>
                    <div style="` + reportCenterTextStyle + `;` + boldTextStyle +`">` + result.incomeTotal.description + `</div>
                    <br> `;
                }

                let initialAmounts: string = '';
                result.initialAmounts.forEach(function (initialAmount) {  
                    let newLine =                     
                    `
                    <li style="` + listItemStyle + `">
                        <span>`+ initialAmount.description + `</span> 
                        <span style="`+ reportFloatRightTextStyle +`;`+ boldTextStyle +`">`+ initialAmount.amount + `</span>
                    </li>`;

                    initialAmounts = initialAmounts.length > 0 ? initialAmounts + newLine : newLine; 
                });  

                let expenses : string = '';
                result.expenses.forEach(function (expense) {  
                    let newLine = 
                    `<li style="` + listItemStyle + `">
                        <span>` + expense.description +`</span>
                        <span style="` + reportFloatRightTextStyle + boldTextStyle + `">` + expense.amount + `</span>
                    </li>`;
                    
                    expenses = expenses.length > 0 ? expenses + newLine : newLine; 
                });  

                let finallyAmounts: string = '';
                result.finallyAmounts.forEach(function (finallyAmount) {  
                    let newLine =                     
                    `<li style="` + listItemStyle + `">
                        <span>`+ finallyAmount.description + `</span> 
                        <span style="`+ reportFloatRightTextStyle +`;`+ boldTextStyle +`">`+ finallyAmount.amount + `</span>
                    </li>`;

                    finallyAmounts = finallyAmounts.length > 0 ? finallyAmounts + newLine : newLine; 
                });  

                this.value = 
                titles + `
                <div style="` + reportLeftTextStyle + `;`+ boldTextStyle + `">` + result.incomesTitle + `</div>
                <ul>
                ` + incomes + `
                </ul>
                ` + incomeTotal + `
                <br>
                <ul style="list-style-type:none;">
                ` + initialAmounts + `
                </ul>
                <div style="` + reportFloatRightTextStyle + `;`+ boldTextStyle + `">` + result.incomeTotalWithInitialAmounts.amount + `</div>
                <div style="` + reportCenterTextStyle + `;`+ boldTextStyle + `">` + result.incomeTotalWithInitialAmounts.description + `</div>
                <br>
                <div style="` + reportLeftTextStyle + `;`+ boldTextStyle + `">` + result.expensesTitle + `</div>
                <ul>
                ` + expenses + `
                </ul>
                <div style="` + reportFloatRightTextStyle + boldTextStyle +`">`+ result.expenseTotal.amount + `</div>
                <div style="` + reportCenterTextStyle + boldTextStyle +`">` + result.expenseTotal.description + `</div>
                <br>
                <ul style="list-style-type:none;">
                ` + finallyAmounts + `
                </ul>                
                <div style="` + reportFloatRightTextStyle + boldTextStyle +`">`+ result.expenseTotalWithFinallyAmounts.amount + `</div>
                <div style="` + reportCenterTextStyle + boldTextStyle +`">` + result.expenseTotalWithFinallyAmounts.description + `</div>`;
            });
    }
}
