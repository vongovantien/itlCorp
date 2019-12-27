import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { PaginationModule, TabsModule, ModalModule } from 'ngx-bootstrap';
import { SelectModule } from 'ng2-select';

import { SharedModule } from 'src/app/shared/shared.module';
import { ShareBussinessModule } from 'src/app/business-modules/share-business/share-bussines.module';
import { ChargeConstants } from 'src/constants/charge.const';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { AirExportHBLComponent } from './air-export-hbl.component';
import { AirExportCreateHBLComponent } from './create/create-house-bill.component';
import { AirExportHBLFormCreateComponent } from './components/form-create-house-bill-air-export/form-create-house-bill-air-export.component';


const routing: Routes = [
    {
        path: '', component: AirExportHBLComponent,
        data: { name: 'House Bill List', path: 'hbl', level: 4, serviceId: ChargeConstants.SFE_CODE }
    },
    {
        path: 'new', component: AirExportCreateHBLComponent,
        data: { name: 'New House Bill Detail', path: ':id', level: 5 }
    },
    // {
    //     path: ':hblId', component: SeaFCLExportDetailHBLComponent,
    //     data: { name: 'House Bill Detail', path: ':id', level: 5 }
    // }
];

const LIB = [
    PaginationModule.forRoot(),
    ModalModule.forRoot(),
    TabsModule.forRoot(),
    SelectModule,
    NgxDaterangepickerMd.forRoot()

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
        AirExportHBLComponent,
        AirExportCreateHBLComponent,
        AirExportHBLFormCreateComponent
    ],
    providers: [],
})
export class AirExportHBLModule { }
