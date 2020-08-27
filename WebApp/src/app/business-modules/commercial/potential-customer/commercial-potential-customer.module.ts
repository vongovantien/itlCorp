import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SelectModule } from 'ng2-select';

import { SharedModule } from 'src/app/shared/shared.module';


import { TabsModule, PaginationModule } from 'ngx-bootstrap';
import { CommercialPotentialCustomerComponent } from './commercial-potential-customer.component';


const routing: Routes = [
    {
        path: '', data: { name: "", title: 'eFMS Commercial Potential Customer' }, children: [
            {
                path: '', component: CommercialPotentialCustomerComponent
            },

        ]
    }
];

@NgModule({
    declarations: [
        CommercialPotentialCustomerComponent,
    ],
    imports: [
        SharedModule,
        CommonModule,
        FormsModule,
        SelectModule,
        ReactiveFormsModule,
        TabsModule.forRoot(),
        RouterModule.forChild(routing),
        PaginationModule.forRoot(),
    ],
    exports: [],
    providers: [],
})
export class CommercialPotentialCustomerModule { }
