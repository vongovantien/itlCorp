import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { SeaFCLExportHBLComponent } from './sea-fcl-export-hbl.component';
import { CommonModule } from '@angular/common';
import { PaginationModule, TabsModule, ModalModule } from 'ngx-bootstrap';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormsModule } from '@angular/forms';
import { ShareBussinessModule } from 'src/app/business-modules/share-business/share-bussines.module';
import { ChargeConstants } from 'src/constants/charge.const';


const routing: Routes = [
    {
        path: '', component: SeaFCLExportHBLComponent,
        data: { name: 'House Bill List', path: 'hbl', level: 4, serviceId: ChargeConstants.SFE_CODE }
    },
    {
        path: 'new', component: SeaFCLExportHBLComponent,
        data: { name: 'New House Bill Detail', path: ':id', level: 5 }
    },
    {
        path: ':hblId', component: SeaFCLExportHBLComponent,
        data: { name: 'House Bill Detail', path: ':id', level: 5 }
    }
];

const LIB = [
    PaginationModule.forRoot(),
    ModalModule.forRoot(),
    TabsModule.forRoot(),
];

@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        ShareBussinessModule,
        FormsModule,
        RouterModule.forChild(routing),

        ...LIB

    ],
    exports: [],
    declarations: [
        SeaFCLExportHBLComponent
    ],
    providers: [],
    bootstrap: [
        SeaFCLExportHBLComponent
    ]
})
export class SeaFCLExportHBLModule { }
