import { NgModule } from '@angular/core';
import { SharedModule } from 'src/app/shared/shared.module';
import { Routes, RouterModule } from '@angular/router';
import { SelectModule } from 'ng2-select';

import { PaginationModule } from 'ngx-bootstrap/pagination';
import { ModalModule } from 'ngx-bootstrap/modal';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SaleReportComponent } from './sale-report.component';
import { SaleReportFormSearchComponent } from './components/form-search-sale-report/form-search-sale-report.component';
const routing: Routes = [
    {
        path: '', data: { name: "" },
        children: [
            {
                path: '', component: SaleReportComponent
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
        SaleReportComponent,
        SaleReportFormSearchComponent
    ],
    providers: [],
})
export class SaleReportModule { }
