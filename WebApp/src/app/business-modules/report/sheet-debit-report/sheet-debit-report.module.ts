import { NgModule } from '@angular/core';
import { SharedModule } from 'src/app/shared/shared.module';
import { Routes, RouterModule } from '@angular/router';
import { SelectModule } from 'ng2-select';

import { PaginationModule } from 'ngx-bootstrap/pagination';
import { ModalModule } from 'ngx-bootstrap/modal';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SheetDebitReportComponent } from './sheet-debit-report.component';
import { SheetDebitReportFormSearchComponent } from './components/form-search-sheet-debit-report/form-search-sheet-debit-report.component';
const routing: Routes = [
    {
        path: '', data: { name: "" },
        children: [
            {
                path: '', component: SheetDebitReportComponent
            }
        ]
    },
]
@NgModule({
    imports: [
        SharedModule,
        SelectModule,
        PaginationModule.forRoot(),
        RouterModule.forChild(routing),
        NgxDaterangepickerMd,
        ModalModule.forRoot(),
    ],
    exports: [],
    declarations: [
        SheetDebitReportComponent,
        SheetDebitReportFormSearchComponent
    ],
    providers: [],
})
export class SheetDebitReportModule { }
