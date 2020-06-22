import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { CommercialCustomerComponent } from './commercial-customer.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { ShareCommercialModule } from '../share-commercial.module';
import { CommercialCreateComponent } from '../create/create-commercial.component';
import { CommercialDetailComponent } from '../detail/detail-commercial.component';
import { CommercialCreateContractComponent } from '../components/contract/components/create-contract/create-contract.component';
import { CommercialDetailContractComponent } from '../components/contract/components/detail-contract/detail-contract.component';

const routing: Routes = [
    {
        path: '', data: { name: "", title: 'eFMS Commercial Customer' }, children: [
            {
                path: '', component: CommercialCustomerComponent
            },
            {
                path: 'new', component: CommercialCreateComponent, data: { name: 'New' }
            },
            {
                path: ':partnerId', component: CommercialDetailComponent, data: { name: 'View/Edit Customer' }
            },
            {
                path: ':partnerId/contract/new', component: CommercialCreateContractComponent, data: { name: 'Create Contract Info' }
            },
            {
                path: ':partnerId/contract/:contractId', component: CommercialDetailContractComponent, data: { name: 'Customer - Contract Info' }
            },

        ]
    }
];


@NgModule({
    declarations: [
        CommercialCustomerComponent,
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

