import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SeaFCLImportHBLComponent } from './sea-fcl-import-hbl.component';
import { Routes, RouterModule } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { TabsModule } from 'ngx-bootstrap';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CreateHouseBillComponent } from './create/create-house-bill.component';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { FormAddHouseBillComponent } from './components/form-add-house-bill/form-add-house-bill.component';
import { FCLImportShareModule } from '../../share-fcl-import.module';

const routing: Routes = [
    {
        path: '', component: SeaFCLImportHBLComponent,
        data: { name: 'House Bill List', path: 'hbl', level: 4 }
    },
    {
        path: 'new', component: CreateHouseBillComponent,
        data: { name: 'New House Bill Detail', path: ':id', level: 5 }
    },
    {
        path: ':id', component: SeaFCLImportHBLComponent,
        data: { name: 'House Bill Detail', path: ':id', level: 5 }
    }
];



const COMPONENTS = [

];

@NgModule({
    declarations: [
        SeaFCLImportHBLComponent,
        ...COMPONENTS,
        CreateHouseBillComponent,
        FormAddHouseBillComponent
    ],
    imports: [
        CommonModule,
        SharedModule,
        FormsModule,
        TabsModule.forRoot(),
        ReactiveFormsModule,
        RouterModule.forChild(routing),
        NgxDaterangepickerMd,
        FCLImportShareModule
    ],
    exports: [],
    providers: [],
})
export class SeaFCLImportHBLModule { }