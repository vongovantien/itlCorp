import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { CommercialAgentComponent } from './commercial-agent.component';

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
        RouterModule.forChild(routing)
    ],
    exports: [],
    providers: [],
})
export class CommercialAgentModule { }
