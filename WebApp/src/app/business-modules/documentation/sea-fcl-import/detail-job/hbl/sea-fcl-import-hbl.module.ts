import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SeaFCLImportHBLComponent } from './sea-fcl-import-hbl.component';
import { Routes, RouterModule } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { TabsModule } from 'ngx-bootstrap';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CreateHouseBillComponent } from './create/create-house-bill.component';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';

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
        CreateHouseBillComponent
    ],
    imports: [
        CommonModule,
        SharedModule,
        FormsModule,
        TabsModule.forRoot(),
        ReactiveFormsModule,
        RouterModule.forChild(routing),
        NgxDaterangepickerMd
    ],
    exports: [],
    providers: [],
})
export class SeaFCLImportHBLModule { }