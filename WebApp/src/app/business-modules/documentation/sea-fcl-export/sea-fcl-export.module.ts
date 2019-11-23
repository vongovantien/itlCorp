import { NgModule } from '@angular/core';

import { SeaFCLExportComponent } from './sea-fcl-export.component';
import { Routes, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SelectModule } from 'ng2-select';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import { SeaFCLExportFormSearchComponent } from './components/form-search/form-search-sea-fcl-export.component';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';
import { effects, reducers } from './store';
import { SeaFCLExportCreateJobComponent } from './create-job/create-job-fcl-export.component';
import { TabsModule } from 'ngx-bootstrap';
import { ShareBussinessModule } from '../../share-business/share-bussines.module';
import { SeaFCLExportFormCreateComponent } from './components/form-create/form-create-fcl-export.component';
import { SeaFCLExportDetailJobComponent } from './detail-job/detail-job-fcl-export.component';

const routing: Routes = [
    {
        path: '', pathMatch: 'full', component: SeaFCLExportComponent, data: {
            name: "Sea FCL Export", path: "sea-fcl-export", level: 2
        },
    },
    {
        path: 'new', component: SeaFCLExportCreateJobComponent, data: {
            name: "Create New Job", path: "sea-fcl-export", level: 3
        }
    },
    {
        path: ':jobId', component: SeaFCLExportDetailJobComponent, data: {
            name: "Job Detail", path: "sea-fcl-export", level: 3
        }
    }

];

const LIB = [
    SelectModule,
    NgxDaterangepickerMd.forRoot(),
    PerfectScrollbarModule,
    TabsModule.forRoot(),

];

const COMPONENTS = [
    SeaFCLExportFormSearchComponent,
    SeaFCLExportFormCreateComponent
];

@NgModule({
    imports: [
        CommonModule,
        RouterModule.forChild(routing),
        FormsModule,
        ReactiveFormsModule,
        SharedModule,
        ShareBussinessModule,
        StoreModule.forFeature('seaFCLExport', reducers),
        EffectsModule.forFeature(effects),
        ...LIB
    ],
    exports: [],
    declarations: [
        SeaFCLExportComponent,
        SeaFCLExportCreateJobComponent,
        SeaFCLExportDetailJobComponent,
        ...COMPONENTS
    ],
    providers: [],
    bootstrap: [
        SeaFCLExportComponent
    ]
})
export class SeaFCLExportModule { }
