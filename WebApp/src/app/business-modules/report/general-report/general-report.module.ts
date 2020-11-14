import { NgModule } from '@angular/core';
import { SharedModule } from 'src/app/shared/shared.module';
import { Routes, RouterModule } from '@angular/router';
import { SelectModule } from 'ng2-select';

import { PaginationModule } from 'ngx-bootstrap/pagination';
import { ModalModule } from 'ngx-bootstrap/modal';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';

import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { GeneralReportComponent } from './general-report.component';
import { GeneralReportFormSearchComponent } from './components/form-search-general-report/form-search-general-report.component';
const routing: Routes = [
    {
        path: '', data: { name: "" },
        children: [
            {
                path: '', component: GeneralReportComponent
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
        BsDropdownModule.forRoot()
    ],
    exports: [],
    declarations: [
        GeneralReportComponent,
        GeneralReportFormSearchComponent
    ],
    providers: [],
})
export class GeneralReportModule { }
