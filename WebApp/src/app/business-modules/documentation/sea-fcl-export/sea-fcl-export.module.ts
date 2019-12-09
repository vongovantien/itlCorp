import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { TabsModule, CollapseModule, PaginationModule } from 'ngx-bootstrap';
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
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { SeaFclExportShippingInstructionComponent } from './detail-job/shipping-instruction/sea-fcl-export-shipping-instruction.component';
import { SeaFclExportBillInstructionComponent } from './detail-job/shipping-instruction/bill-instruction/sea-fcl-export-bill-instruction.component';
import { SeaFclExportBillDetailComponent } from './detail-job/shipping-instruction/bill-detail/sea-fcl-export-bill-detail.component';
import { SeaFclExportManifestComponent } from './detail-job/manifest/sea-fcl-export-manifest.component';

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
            name: "Job Detail", path: "sea-fcl-export", level: 3, transactionType: CommonEnum.TransactionTypeEnum.SeaFCLExport
        }
    },
    {
        path: ':jobId/hbl', loadChildren: () => import('./detail-job/hbl/sea-fcl-export-hbl.module').then(m => m.SeaFCLExportHBLModule),
    },
    {
        path: ':jobId/si', component: SeaFclExportShippingInstructionComponent, data: {
            name: "Shipping Instructions", path: ":jobId", level: 4
        }
    },
    {
        path: ':id/manifest', component: SeaFclExportManifestComponent,
        data: { name: "Manifest", path: ":id", level: 4 },
    },
];

const LIB = [
    SelectModule,
    NgxDaterangepickerMd.forRoot(),
    PerfectScrollbarModule,
    TabsModule.forRoot(),
    CollapseModule.forRoot(),
    PaginationModule.forRoot(),
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
        SeaFclExportShippingInstructionComponent,
        SeaFclExportBillInstructionComponent,
        SeaFclExportBillDetailComponent,
        SeaFclExportManifestComponent,
        ...COMPONENTS
    ],
    providers: [],
})
export class SeaFCLExportModule { }
