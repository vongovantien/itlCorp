import { NgModule } from '@angular/core';

import { CurrencyComponent } from './currency.component';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/shared/shared.module';
import { SelectModule } from 'ng2-select';
import { FormsModule } from '@angular/forms';
import { NgProgressModule } from '@ngx-progressbar/core';
import { Routes, RouterModule } from '@angular/router';

const routing: Routes = [
    { path: '', component: CurrencyComponent, data: { name: "Currency", level: 2 } }
];

@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        SelectModule,
        FormsModule,
        NgProgressModule,
        RouterModule.forChild(routing)
    ],
    exports: [],
    declarations: [CurrencyComponent],
    providers: [],
})
export class CurrencyModule { }
