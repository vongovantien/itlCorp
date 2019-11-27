import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { TabsModule } from 'ngx-bootstrap';
import { SelectModule } from 'ng2-select';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';

import { SeaFCLExportComponent } from './sea-fcl-export.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { SeaFCLExportFormSearchComponent } from './components/form-search/form-search-sea-fcl-export.component';
import { SeaFCLExportCreateJobComponent } from './create-job/create-job-fcl-export.component';
import { ShareBussinessModule } from '../../share-business/share-bussines.module';
import { SeaFCLExportFormCreateComponent } from './components/form-create/form-create-fcl-export.component';
import { SeaFCLExportDetailJobComponent } from './detail-job/detail-job-fcl-export.component';
import { SeaFCLExportLazyLoadModule } from './sea-fcl-export-lazy-load.module';

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
    },
    {
        path: ':jobId/hbl', loadChildren: () => import('./detail-job/hbl/sea-fcl-export-hbl.module').then(m => m.SeaFCLExportHBLModule),
    },
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
        ...LIB,
        SeaFCLExportLazyLoadModule // ?  Lazy loading module with  tab component (CD Note)
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
