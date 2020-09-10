import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgSelectModule } from '@ng-select/ng-select';

import { SharedModule } from 'src/app/shared/shared.module';


import { TabsModule, PaginationModule, ModalModule } from 'ngx-bootstrap';
import { CommercialPotentialCustomerComponent } from './commercial-potential-customer.component';
import { CommercialFormSearchPotentialCustomerComponent } from './components/form-search/form-search-potential-customer.component';
import { CommercialPotentialCustomerPopupComponent } from './components/popup/potential-customer-commercial.popup';


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
        CommercialFormSearchPotentialCustomerComponent,
        CommercialPotentialCustomerPopupComponent,
    ],
    imports: [
        SharedModule,
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        TabsModule.forRoot(),
        RouterModule.forChild(routing),
        PaginationModule.forRoot(),
        ModalModule.forRoot(),
        NgSelectModule,
    ],
    exports: [],
    providers: [],
})
export class CommercialPotentialCustomerModule { }
