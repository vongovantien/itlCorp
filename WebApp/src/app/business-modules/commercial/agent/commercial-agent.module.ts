import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { ShareModulesModule } from '../../share-modules/share-modules.module';
import { ContractImportComponent } from '../components/contract/import/contract-import.component';
import { CustomerAgentImportComponent } from '../components/customer-agent-import/customer-agent-import.component';
import { CommercialCreateComponent } from '../create/create-commercial.component';
import { CommercialDetailComponent } from '../detail/detail-commercial.component';
import { ShareCommercialModule } from '../share-commercial.module';
import { CommercialAgentComponent } from './commercial-agent.component';
const routing: Routes = [
    {
        path: '', data: { name: "" }, children: [
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
                path: 'new-sub/:partnerId', component: CommercialDetailComponent, data: { name: 'New SubAgent', path: ':partnerId', action: true }
            },
        ]
    }
];


@NgModule({
    declarations: [
        CommercialAgentComponent,

    ],
    imports: [
        RouterModule.forChild(routing),
        SharedModule,
        ShareCommercialModule,
        ShareModulesModule,
    ],
    exports: [],
    providers: [],
})
export class CommercialAgentModule { }
