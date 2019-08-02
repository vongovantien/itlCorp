import { NgModule } from '@angular/core';

import { StageManagementComponent } from './stage-management.component';
import { RouterModule, Routes } from '@angular/router';
import { StageImportComponent } from '../stage-import/stage-import.component';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'ng2-select';
import { NgProgressModule } from '@ngx-progressbar/core';


const routing: Routes = [
    { path: '', component: StageManagementComponent, data: { name: "Stage Management", level: 2 } },
    { path: 'stage-import', component: StageImportComponent, data: { name: "Stage Import", level: 3 } },
];

@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        FormsModule,
        SelectModule,
        NgProgressModule,
        RouterModule.forChild(routing)
    ],
    exports: [],
    declarations: [
        StageManagementComponent,
        StageImportComponent
    ],
    providers: [],
})
export class StateManagementModule { }
