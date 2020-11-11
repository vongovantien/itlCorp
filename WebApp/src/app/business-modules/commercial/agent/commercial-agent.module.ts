import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { CommercialAgentComponent } from './commercial-agent.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { CommercialDetailComponent } from '../detail/detail-commercial.component';
import { CommercialCreateComponent } from '../create/create-commercial.component';
import { ShareCommercialModule } from '../share-commercial.module';
import { ContractImportComponent } from '../components/contract/import/contract-import.component';
import { CustomerAgentImportComponent } from '../components/customer-agent-import/customer-agent-import.component';

const routing: Routes = [
    {
        path: '', data: { name: "", title: 'eFMS Commercial Agent' }, children: [
            {
                path: '', component: CommercialAgentComponent
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
                path: ':partnerId', component: CommercialDetailComponent, data: { name: 'View/Edit Agent' }
            },
            {
                path: 'new/:partnerId/:isAddSub', component: CommercialDetailComponent, data: { name: 'New Branch/Sub', path: ':partnerId' }
            },
        ]
    }
];


@NgModule({
    declarations: [
        CommercialAgentComponent,

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
export class CommercialAgentModule { }
