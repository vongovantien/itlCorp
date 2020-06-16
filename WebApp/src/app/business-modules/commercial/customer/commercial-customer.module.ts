import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { CommercialCustomerComponent } from './commercial-customer.component';
import { SharedModule } from 'src/app/shared/shared.module';

const routing: Routes = [
    {
        path: '', data: { name: "", title: 'eFMS Commercial Customer' }, children: [
            {
                path: '', component: CommercialCustomerComponent
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
        SharedModule
    ],
    exports: [],
    providers: [],
})
export class CommercialCustomerModule { }

