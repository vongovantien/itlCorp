import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Routes, RouterModule } from '@angular/router';

import { SharedModule } from 'src/app/shared/shared.module';
import { TabsModule, PaginationModule, ModalModule, CollapseModule } from 'ngx-bootstrap';
import { SelectModule } from 'ng2-select';
import { SeaFCLImportManagementComponent } from './sea-fcl-import-management.component';
import { SeaFCLImportCreateJobComponent } from './create-job/create-job-fcl-import.component';
import { SeaFCLImportDetailJobComponent } from './detail-job/detail-job-fcl-import.component';
import { SeaFCLImportManagementFormSearchComponent } from './components/form-search/form-search-fcl-import.component';
import { SeaFClImportFormCreateComponent } from './components/form-create/form-create-sea-fcl-import.component';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { StoreModule } from '@ngrx/store';
import { reducers, effects } from './store';
import { EffectsModule } from '@ngrx/effects';
import { FCLImportShareModule } from './share-fcl-import.module';
import { SeaFCLImportGrantTotalProfitComponent } from './components/grant-total-profit/grant-total-profit.component';
import { SeaFCLImportLazyLoadModule } from './sea-fcl-import-lazy-load.module';
import { SeaFclImportManifestComponent } from './detail-job/manifest/sea-fcl-import-manifest.component';
import { FormManifestSeaFclImportComponent } from './detail-job/manifest/components/form-manifest/form-manifest-sea-fcl-import.component';
import { AddHblToManifestComponent } from './detail-job/manifest/popup/add-hbl-to-manifest.popup';

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
        path: ':id', component: SeaFCLImportDetailJobComponent,
        data: { name: "Job Detail", path: ":id", level: 3 },
    },
    {
        path: ':id/hbl', loadChildren: () => import('./detail-job/hbl/sea-fcl-import-hbl.module').then(m => m.SeaFCLImportHBLModule),
    },
    {
        path: ':id/manifest', component: SeaFclImportManifestComponent,
        data: { name: "Manifest", path: ":id", level: 3 },
    },


];

const COMPONENTS = [
    SeaFCLImportManagementFormSearchComponent,
    SeaFClImportFormCreateComponent,
    SeaFCLImportGrantTotalProfitComponent,
    AddHblToManifestComponent

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
        ...COMPONENTS,
        SeaFCLImportManagementComponent,
        SeaFCLImportCreateJobComponent,
        SeaFCLImportDetailJobComponent,
        SeaFclImportManifestComponent,
        FormManifestSeaFclImportComponent
    ],
    imports: [
        FCLImportShareModule,
        CommonModule,
        SharedModule,
        RouterModule.forChild(routing),
        FormsModule,
        ReactiveFormsModule,
        ...LIB,
        StoreModule.forFeature('seaFClImport', reducers),
        EffectsModule.forFeature(effects),
        SeaFCLImportLazyLoadModule // ?  Lazy loading module with  tab component (CD Note, Assignment).

    ],
    exports: [],
    providers: [],
    bootstrap: [
        SeaFCLImportManagementComponent,
    ]
})
export class SeaFCLImportModule { }
