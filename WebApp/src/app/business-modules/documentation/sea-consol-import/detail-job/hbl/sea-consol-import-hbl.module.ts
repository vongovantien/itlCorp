import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { TabsModule } from 'ngx-bootstrap/tabs';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { NgxSpinnerModule } from 'ngx-spinner';

import { SharedModule } from 'src/app/shared/shared.module';
import { ShareBussinessModule } from 'src/app/business-modules/share-business/share-bussines.module';
import { ChargeConstants } from 'src/constants/charge.const';

import { SeaConsolImportHBLComponent } from './sea-consol-import-hbl.component';
import { SeaConsolImportCreateHBLComponent } from './create/create-hbl-consol-import.component';
import { SeaConsolImportDetailHBLComponent } from './detail/detail-hbl-consol-import.component';
import { ShareBusinessReAlertComponent } from 'src/app/business-modules/share-business/components/pre-alert/pre-alert.component';

const routing: Routes = [
    {
        path: '', component: SeaConsolImportHBLComponent,
        data: <CommonInterface.IDataParam>{ name: '', path: 'hbl', level: 4, serviceId: ChargeConstants.SCI_CODE }
    },
    {
        path: 'new', component: SeaConsolImportCreateHBLComponent,
        data: { name: 'New House Bill', path: ':id', level: 5 }
    },
    {
        path: ':hblId',
        data: { name: 'House Bill Detail', path: ':id', level: 5 },
        children: [
            {
                path: '', component: SeaConsolImportDetailHBLComponent, data: { name: "" }
            },
            {
                path: 'arrivalnotice', component: ShareBusinessReAlertComponent,
                data: { name: "Arrival Notice", level: 6, serviceId: ChargeConstants.SFI_CODE },
            },
        ]
    }
];

const LIB = [
    TabsModule.forRoot(),
    BsDropdownModule.forRoot()
];


@NgModule({
    declarations: [
        SeaConsolImportHBLComponent,
        SeaConsolImportCreateHBLComponent,
        SeaConsolImportDetailHBLComponent
    ],
    imports: [
        CommonModule,
        SharedModule,
        ShareBussinessModule,
        FormsModule,
        ReactiveFormsModule,
        RouterModule.forChild(routing),
        NgxSpinnerModule,
        ...LIB

    ],
    exports: [],
    providers: [],

})
export class SeaConsolImportHBLModule { }

