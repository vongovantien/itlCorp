import { NgModule } from '@angular/core';

import { UnitComponent } from './unit.component';
import { Routes, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'ng2-select';
import { NgProgressModule } from '@ngx-progressbar/core';

const routing: Routes = [
    { path: '', component: UnitComponent, data: { name: "Unit", level: 2 } },
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
        UnitComponent
    ],
    providers: [],
})
export class UnitModule { }
