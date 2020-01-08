import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Routes, RouterModule } from '@angular/router';
import { SelectModule } from 'ng2-select';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { TabsModule, PaginationModule, ModalModule, CollapseModule } from 'ngx-bootstrap';

import { SharedModule } from 'src/app/shared/shared.module';
import { SeaFCLImportManagementComponent } from './sea-fcl-import-management.component';
import { SeaFCLImportCreateJobComponent } from './create-job/create-job-fcl-import.component';
import { SeaFCLImportDetailJobComponent } from './detail-job/detail-job-fcl-import.component';
import { SeaFCLImportLazyLoadModule } from './sea-fcl-import-lazy-load.module';
import { SeaFclImportManifestComponent } from './detail-job/manifest/sea-fcl-import-manifest.component';
import { ShareBussinessModule } from '../../share-business/share-bussines.module';
import { CommonEnum } from 'src/app/shared/enums/common.enum';

const routing: Routes = [

    {
        path: '', pathMatch: 'full', component: SeaFCLImportManagementComponent,
        data: { name: "Sea FCL Import", path: "sea-fcl-import", level: 2 }
    },
    {
        path: 'new', component: SeaFCLImportCreateJobComponent,
        data: { name: "Create New Job", path: "new", level: 3 }
    },
    {
        path: ':jobId', component: SeaFCLImportDetailJobComponent,
        data: { name: "Job Detail", path: ":jobId", level: 3, transactionType: CommonEnum.TransactionTypeEnum.SeaFCLImport },
    },
    {
        path: ':jobId/hbl', loadChildren: () => import('./detail-job/hbl/sea-fcl-import-hbl.module').then(m => m.SeaFCLImportHBLModule),
    },
    {
        path: ':jobId/manifest', component: SeaFclImportManifestComponent,
        data: { name: "Manifest", path: ":jobId", level: 4 },
    },


];


const LIB = [
    CollapseModule.forRoot(),
    TabsModule.forRoot(),
    ModalModule.forRoot(),
    PaginationModule.forRoot(),
    SelectModule,
    NgxDaterangepickerMd.forRoot()
];


@NgModule({
    declarations: [
        SeaFCLImportManagementComponent,
        SeaFCLImportCreateJobComponent,
        SeaFCLImportDetailJobComponent,
        SeaFclImportManifestComponent
    ],
    imports: [
        CommonModule,
        SharedModule,
        RouterModule.forChild(routing),
        FormsModule,
        ReactiveFormsModule,
        ...LIB,
        SeaFCLImportLazyLoadModule, // ?  Lazy loading module with  tab component (CD Note, Assignment).
        ShareBussinessModule

    ],
    exports: [],
    providers: [],
})
export class SeaFCLImportModule { }
