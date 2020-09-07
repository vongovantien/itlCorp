import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Routes, RouterModule } from '@angular/router';

import { PaginationModule } from 'ngx-bootstrap/pagination';
import { ModalModule } from 'ngx-bootstrap/modal';
import { NgProgressModule } from '@ngx-progressbar/core';

import { CurrencyComponent } from './currency.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormCreateCurrencyPopupComponent } from './components/form-create/form-create-currency.popup';

const routing: Routes = [
    { path: '', component: CurrencyComponent, data: { name: "", title: 'eFMS Currency' } }
];

@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        FormsModule,
        ReactiveFormsModule,
        ModalModule.forRoot(),
        NgProgressModule,
        PaginationModule.forRoot(),
        RouterModule.forChild(routing)
    ],
    exports: [],
    declarations: [
        CurrencyComponent,
        FormCreateCurrencyPopupComponent
    ],
    providers: [],
})
export class CurrencyModule { }
