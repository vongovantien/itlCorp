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
import { AirImportComponent } from './air-import.component';
import { AirImportLazyLoadModule } from './air-import-lazy-load.module';

const routing: Routes = [
    {
        path: '', pathMatch: 'full', component: AirImportComponent, data: {
            name: "Air Export", path: "air-export", level: 2
        },
    },
    // {
    //     path: 'new', component: AirImportCreateJobComponent,
    //     data: { name: "Create New Job", path: "new", level: 3 }
    // },
    // {
    //     path: ':jobId', component: AirImportDetailJobComponent,
    //     data: { name: "Job Detail", path: ":id", level: 3, transactionType: CommonEnum.TransactionTypeEnum.AirImport },
    // },
    // {
    //     path: ':jobId/hbl', loadChildren: () => import('./detail-job/hbl/Air-import-hbl.module').then(m => m.AirImportHBLModule),
    // },
    // {
    //     path: ':id/manifest', component: AirImportManifestComponent,
    //     data: { name: "Manifest", path: ":id", level: 4 },
    // },
    // {
    //     path: ':jobId/si', component: AirImportShippingInstructionComponent, data: {
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
        AirImportLazyLoadModule,
        ...LIB,
    ],
    exports: [],
    declarations: [
        AirImportComponent
    ],
    providers: [],
})
export class AirImportModule { }
