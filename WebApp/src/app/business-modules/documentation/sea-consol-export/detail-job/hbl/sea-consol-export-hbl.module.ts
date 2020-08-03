import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { PaginationModule, TabsModule, ModalModule } from 'ngx-bootstrap';
import { SelectModule } from 'ng2-select';
import { ChargeConstants } from '@constants';
import { NgxSpinnerModule } from 'ngx-spinner';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';

import { SeaConsolExportHBLComponent } from './sea-consol-export-hbl.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { ShareBussinessModule } from 'src/app/business-modules/share-business/share-bussines.module';
import { SeaConsolExportCreateHBLComponent } from './create/create-hbl-consol-export.component';
import { SeaConsolExportDetailHBLComponent } from './detail/detail-hbl-consol-export.component';


const routing: Routes = [
    {
        path: '', component: SeaConsolExportHBLComponent,
        data: { name: '', path: 'hbl', level: 4, serviceId: ChargeConstants.SFE_CODE }
    },
    {
        path: 'new', component: SeaConsolExportCreateHBLComponent,
        data: { name: 'New House Bill', path: ':id', level: 5 }
    },
    {
        path: ':hblId', component: SeaConsolExportDetailHBLComponent,
        data: { name: 'House Bill Detail', path: ':id', level: 5 }
    }
];

const LIB = [
    // PaginationModule.forRoot(),
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
        NgxSpinnerModule,
        ...LIB

    ],
    exports: [],
    declarations: [
        SeaConsolExportHBLComponent,
        SeaConsolExportCreateHBLComponent,
        SeaConsolExportDetailHBLComponent
    ],
    providers: [],
})
export class SeaConsolExportHBLModule { }
