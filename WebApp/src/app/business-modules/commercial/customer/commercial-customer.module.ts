import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { CommercialCustomerComponent } from './commercial-customer.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { ShareCommercialModule } from '../share-commercial.module';
import { CommercialCreateComponent } from '../create/create-commercial.component';

const routing: Routes = [
    {
        path: '', data: { name: "", title: 'eFMS Commercial Customer' }, children: [
            {
                path: '', component: CommercialCustomerComponent
            },
            {
                path: 'new', component: CommercialCreateComponent, data: { name: 'New' }
            },

        ]
    }
];


@NgModule({
    declarations: [
        CommercialCustomerComponent
    ],
    imports: [
        CommonModule,
        RouterModule.forChild(routing),
        SharedModule,
        ShareCommercialModule
    ],
    exports: [],
    providers: [],
})
export class CommercialCustomerModule { }

