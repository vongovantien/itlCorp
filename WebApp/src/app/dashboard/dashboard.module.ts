import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HighchartsChartModule } from 'highcharts-angular';
import { SharedModule } from '../shared/shared.module';
import { FormSearchTrackingComponent } from './components/form-search-tracking/form-search-tracking.component';
import { DashboardComponent } from './dashboard.component';

const routing: Routes = [
    {
        path: '', data: { name: "" }, children: [
            {
                path: '', component: DashboardComponent
            }
        ]
    },
];


@NgModule({
    declarations: [
        DashboardComponent,
        FormSearchTrackingComponent
    ],
    imports: [
        RouterModule.forChild(routing),
        CommonModule,
        HighchartsChartModule,
        SharedModule,
    ]
})
export class DashboardModule { }
