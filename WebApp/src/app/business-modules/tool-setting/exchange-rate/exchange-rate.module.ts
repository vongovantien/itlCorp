import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { ExchangeRateComponent } from './exchange-rate.component';
import { SelectModule } from 'ng2-select';
import { FormsModule } from '@angular/forms';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SharedModule } from 'src/app/shared/shared.module';

const routing: Routes = [
    {
        path: '', data: { name: "" }, children: [
            {
                path: '', component: ExchangeRateComponent
            },
        ]
    },
];

@NgModule({
    declarations: [
        ExchangeRateComponent
    ],
    imports: [
        CommonModule,
        SharedModule,
        SelectModule,
        FormsModule,
        NgxDaterangepickerMd,
        RouterModule.forChild(routing)
    ],
    exports: [],
    providers: [],
})
export class ExchangeRateModule { }

