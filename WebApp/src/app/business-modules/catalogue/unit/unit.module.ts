import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { SelectModule } from 'ng2-select';
import { NgProgressModule } from '@ngx-progressbar/core';

import { ModalModule } from 'ngx-bootstrap/modal';
import { PaginationModule } from 'ngx-bootstrap/pagination';

import { UnitComponent } from './unit.component';
import { Routes, RouterModule } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormCreateUnitPopupComponent } from './components/form/form-unit.popup';

const routing: Routes = [
    { path: '', component: UnitComponent, data: { name: "", title: 'eFMS Unit' } },
];

@NgModule({
    imports: [
        CommonModule,
        PaginationModule.forRoot(),
        ModalModule.forRoot(),
        SharedModule,
        FormsModule,
        SelectModule,
        NgProgressModule,
        ReactiveFormsModule,
        RouterModule.forChild(routing)
    ],
    exports: [],
    declarations: [
        UnitComponent,
        FormCreateUnitPopupComponent
    ],
    providers: [],
})
export class UnitModule { }
