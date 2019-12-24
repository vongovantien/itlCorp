import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { TabsModule, CollapseModule, PaginationModule } from 'ngx-bootstrap';
import { SelectModule } from 'ng2-select';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';

import { SharedModule } from 'src/app/shared/shared.module';
import { ShareBussinessModule } from '../../share-business/share-bussines.module';
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { AirExportComponent } from './air-export.component';
import { AirExportLazyLoadModule } from './air-export-lazy-load.module';
import { AirExportCreateJobComponent } from './create-job/create-job-air-export.component';
import { AirExportDetailJobComponent } from './detail-job/detail-job-air-export.component';

const routing: Routes = [
    {
        path: '', pathMatch: 'full', component: AirExportComponent, data: {
            name: "Air Export", path: "air-export", level: 2
        },
    },
    {
        path: 'new', component: AirExportCreateJobComponent,
        data: { name: "Create New Job", path: "new", level: 3 }
    },
    {
        path: ':jobId', component: AirExportDetailJobComponent,
        data: { name: "Job Detail", path: ":id", level: 3, transactionType: CommonEnum.TransactionTypeEnum.AirExport },
    },
    {
        path: ':jobId/hbl', loadChildren: () => import('./detail-job/hbl/air-export-hbl.module').then(m => m.AirExportHBLModule),
    },
    // {
    //     path: ':id/manifest', component: AirExportManifestComponent,
    //     data: { name: "Manifest", path: ":id", level: 4 },
    // },
    // {
    //     path: ':jobId/si', component: AirExportShippingInstructionComponent, data: {
    //         name: "Shipping Instructions", path: ":jobId", level: 4
    //     }
    // }
];

const LIB = [
    SelectModule,
    NgxDaterangepickerMd.forRoot(),
    PerfectScrollbarModule,
    TabsModule.forRoot(),
    CollapseModule.forRoot(),
    PaginationModule.forRoot(),
];

@NgModule({
    imports: [
        CommonModule,
        RouterModule.forChild(routing),
        FormsModule,
        ReactiveFormsModule,
        SharedModule,
        ShareBussinessModule,
        AirExportLazyLoadModule,
        ...LIB,
    ],
    exports: [],
    declarations: [
        AirExportComponent,
        AirExportCreateJobComponent,
        AirExportDetailJobComponent,
    ],
    providers: [],
})
export class AirExportModule { }
