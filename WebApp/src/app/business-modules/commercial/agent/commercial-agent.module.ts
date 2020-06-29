import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { CommercialAgentComponent } from './commercial-agent.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { CommercialDetailComponent } from '../detail/detail-commercial.component';
import { CommercialCreateComponent } from '../create/create-commercial.component';
import { ShareCommercialModule } from '../share-commercial.module';

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
                path: ':partnerId', component: CommercialDetailComponent, data: { name: 'View/Edit Agent' }
            }
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
