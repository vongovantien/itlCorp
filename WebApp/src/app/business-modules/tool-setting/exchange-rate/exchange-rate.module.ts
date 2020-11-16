import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ExchangeRateComponent } from './exchange-rate.component';
import { SelectModule } from 'ng2-select';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SharedModule } from 'src/app/shared/shared.module';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { ModalModule } from 'ngx-bootstrap/modal';
import { ExchangeRateHistoryPopupComponent } from './components/detail-history/exchange-rate-history.popup';
import { ExchangeRateConvertComponent } from './components/convert/exchange-rate-convert.component';
import { ExchangeRateFormComponent } from './components/form-exchange-rate/exchange-rate-form.component';

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
        ExchangeRateComponent,
        ExchangeRateHistoryPopupComponent,
        ExchangeRateConvertComponent,
        ExchangeRateFormComponent
    ],
    imports: [
        SharedModule,
        SelectModule,
        NgxDaterangepickerMd,
        RouterModule.forChild(routing),
        PaginationModule,
        ModalModule
    ],
    exports: [],
    providers: [],
})
export class ExchangeRateModule { }

