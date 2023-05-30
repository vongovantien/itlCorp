import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EffectsModule } from '@ngrx/effects';
import { StoreModule } from '@ngrx/store';
import { SharedModule } from 'src/app/shared/shared.module';
import { ShareModulesModule } from '../../share-modules/share-modules.module';
import { ContractImportComponent } from '../components/contract/import/contract-import.component';
import { CustomerAgentImportComponent } from '../components/customer-agent-import/customer-agent-import.component';
import { CommercialCreateComponent } from '../create/create-commercial.component';
import { CommercialDetailComponent } from '../detail/detail-commercial.component';
import { ShareCommercialModule } from '../share-commercial.module';
import { CommercialCustomerComponent } from './commercial-customer.component';
import { reducers } from '../store';
import { commercialEffect } from '../store/effects';

const routing: Routes = [
    {
        path: '', data: { name: "" },
        children: [
            {
                path: '', component: CommercialCustomerComponent
            },
            {
                path: 'new', component: CommercialCreateComponent, data: { name: 'New' }
            },
            {
                path: 'import', component: CustomerAgentImportComponent, data: { name: "Import", level: 3 }
            },
            {
                path: 'importContract', component: ContractImportComponent, data: { name: "Import Contract", level: 3 }
            },
            {
                path: ':partnerId', component: CommercialDetailComponent, data: { name: 'View/Edit Customer' }
            },
            {
                path: 'new-sub/:partnerId', component: CommercialDetailComponent, data: { name: 'New SubCustomer', path: "partnerId", action: true }
            },
        ]
    }
];


@NgModule({
    declarations: [
        CommercialCustomerComponent
    ],
    imports: [
        RouterModule.forChild(routing),
        SharedModule,
        ShareCommercialModule,
        ShareModulesModule,
        StoreModule.forFeature('commercial', reducers),
        EffectsModule.forFeature(commercialEffect)
    ],
    exports: [],
    providers: [],
})
export class CommercialCustomerModule { }

