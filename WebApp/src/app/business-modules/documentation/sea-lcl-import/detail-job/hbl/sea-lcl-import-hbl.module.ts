import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { SelectModule } from 'ng2-select';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { PaginationModule, ModalModule, TabsModule } from 'ngx-bootstrap';

import { SeaLCLImportHBLComponent } from './sea-lcl-import-hbl.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { ShareBussinessModule } from 'src/app/business-modules/share-business/share-bussines.module';

import { ChargeConstants } from 'src/constants/charge.const';


const routing: Routes = [
    {
        path: '', component: SeaLCLImportHBLComponent,
        data: { name: 'House Bill List', path: 'hbl', level: 4, serviceId: ChargeConstants.SLI_CODE }
    },
    {
        path: 'new', component: SeaLCLImportHBLComponent,
        data: { name: 'New House Bill', path: ':id', level: 5 }
    },
    {
        path: ':hblId', component: SeaLCLImportHBLComponent,
        data: { name: 'House Bill Detail', path: ':id', level: 5 }
    }
];

const LIB = [
    PaginationModule.forRoot(),
    ModalModule.forRoot(),
    TabsModule.forRoot(),
    SelectModule,
    NgxDaterangepickerMd.forRoot()

];

const COMPONENTS = [
];


@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        ShareBussinessModule,
        FormsModule,
        RouterModule.forChild(routing),
        ReactiveFormsModule,
        ...LIB
    ],
    exports: [],
    declarations: [
        SeaLCLImportHBLComponent,
        ...COMPONENTS
    ],
    providers: [],
    bootstrap: [SeaLCLImportHBLComponent]
})
export class SeaLCLImportHBLModule { }
