import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SeaFCLImportManagementComponent } from './sea-fcl-import-management/sea-fcl-import-management.component';
import { Routes, RouterModule } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TabsModule, PaginationModule, ModalModule } from 'ngx-bootstrap';
import { SelectModule } from 'ng2-select';
import { SeaFCLImportManagementFormSearchComponent } from './sea-fcl-import-management/components/form-search/form-search-fcl-import.component';
import { SeaFCLImportCreateJobComponent } from './sea-fcl-import-management/create-job/create-job-fcl-import.component';
import { SeaFClImportFormCreateComponent } from './sea-fcl-import-management/components/form-create/form-create-sea-fcl-import.component';
import { SeaFCLImportShipmentGoodSummaryComponent } from './sea-fcl-import-management/components/shipment-good-summary/shipment-good-summary.component';


const routing: Routes = [
    {
        path: '', pathMatch: 'full', component: SeaFCLImportManagementComponent,
        data: { name: "Sea FCL Import", path: "sea-fcl-import", level: 2 }
    },
    {
        path: 'new', component: SeaFCLImportCreateJobComponent,
        data: { name: "Create New Job", path: "new", level: 3 }
    }
];

const COMPONENTS = [
    SeaFCLImportManagementFormSearchComponent,
    SeaFClImportFormCreateComponent,
    SeaFCLImportShipmentGoodSummaryComponent,
];

@NgModule({
    declarations: [
        ...COMPONENTS,
        SeaFCLImportManagementComponent,
        SeaFCLImportCreateJobComponent
    ],
    imports: [
        CommonModule,
        SharedModule,
        RouterModule.forChild(routing),
        FormsModule,
        ReactiveFormsModule,
        TabsModule,
        PaginationModule,
        SelectModule,
        ModalModule.forRoot()
    ],
    exports: [],
    providers: [],
})
export class SeaFCLImportModule { }
