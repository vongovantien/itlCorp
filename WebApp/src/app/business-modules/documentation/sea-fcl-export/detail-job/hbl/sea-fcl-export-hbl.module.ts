import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { PaginationModule } from 'ngx-bootstrap/pagination';
import { ModalModule } from 'ngx-bootstrap/modal';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { SelectModule } from 'ng2-select';

import { SeaFCLExportHBLComponent } from './sea-fcl-export-hbl.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { ShareBussinessModule } from 'src/app/business-modules/share-business/share-bussines.module';
import { ChargeConstants } from 'src/constants/charge.const';
import { SeaFCLExportCreateHBLComponent } from './create/create-house-bill.component';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SeaFCLExportDetailHBLComponent } from './detail/detail-house-bill.component';
import { NgxSpinnerModule } from 'ngx-spinner';
import { ShareBusinessReAlertComponent } from 'src/app/business-modules/share-business/components/pre-alert/pre-alert.component';

const routing: Routes = [
    {
        path: '', component: SeaFCLExportHBLComponent,
        data: { name: '', path: 'hbl', level: 4, serviceId: ChargeConstants.SFE_CODE }
    },
    {
        path: 'new', component: SeaFCLExportCreateHBLComponent,
        data: { name: 'New House Bill', path: ':id', level: 5 }
    },
    {
        path: ':hblId',
        data: { name: 'House Bill Detail', path: ':id', level: 5 },
        children: [
            {
                path: '', component: SeaFCLExportDetailHBLComponent, data: { name: "" }
            },
            {
                path: 'manifest', component: ShareBusinessReAlertComponent,
                data: { name: "Pre Alert", level: 6, serviceId: ChargeConstants.SFE_CODE },
            }
        ],
    }
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
        SharedModule,
        ShareBussinessModule,
        RouterModule.forChild(routing),
        NgxSpinnerModule,
        ...LIB

    ],
    exports: [],
    declarations: [
        SeaFCLExportHBLComponent,
        SeaFCLExportCreateHBLComponent,
        SeaFCLExportDetailHBLComponent,
    ],
    providers: [],
})
export class SeaFCLExportHBLModule { }
