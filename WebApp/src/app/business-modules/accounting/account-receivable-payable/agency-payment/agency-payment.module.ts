import { NgModule } from '@angular/core';
import { Routes, Route, RouterModule } from '@angular/router';
import { ARAgencyPaymentComponent } from './agency-payment.component';
import { SharedModule } from 'src/app/shared/shared.module';

const routing: Routes = [
    {
        path: '', data: { name: '' }, children: <Route[]>[
            { path: '', component: ARAgencyPaymentComponent }
        ]
    }
];


@NgModule({
    declarations: [
        ARAgencyPaymentComponent,
    ],
    imports: [
        SharedModule,
        RouterModule.forChild(routing)
    ],
    exports: [],
    providers: [],
})
export class ARAgencyPaymentModule { }
