import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { CommercialAgentComponent } from './commercial-agent.component';
import { SharedModule } from 'src/app/shared/shared.module';

const routing: Routes = [
    {
        path: '', data: { name: "", title: 'eFMS Commercial Agent' }, children: [
            {
                path: '', component: CommercialAgentComponent
            },

        ]
    }
];


@NgModule({
    declarations: [
        CommercialAgentComponent
    ],
    imports: [
        CommonModule,
        RouterModule.forChild(routing),
        SharedModule
    ],
    exports: [],
    providers: [],
})
export class CommercialAgentModule { }
