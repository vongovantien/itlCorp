import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from './../shared/shared.module';
import { FormSearchTrackingComponent } from './components/form-search-tracking/form-search-tracking.component';
import { TrackingComponent } from './tracking.component';

const routing: Routes = [
    {
        path: '', data: { name: "" }, children: [
            {
                path: '', component: TrackingComponent
            }
        ]
    },
];

@NgModule({
    declarations: [FormSearchTrackingComponent, TrackingComponent],
    imports: [
        CommonModule,
        RouterModule.forChild(routing),
        CommonModule,
        SharedModule
    ]
})
export class TrackingModule { }
