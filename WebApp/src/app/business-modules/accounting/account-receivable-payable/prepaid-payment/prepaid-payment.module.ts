import { NgModule } from '@angular/core';
import { ARPrePaidPaymentComponent } from './prepaid-payment.component';
import { RouterModule, Routes } from '@angular/router';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { ModalModule } from 'ngx-bootstrap/modal';
import { SharedModule } from 'src/app/shared/shared.module';
import { ARPrePaidPaymentFormSearchComponent } from './components/form-search/form-search-prepaid-payment.component';
import { NgSelectModule } from '@ng-select/ng-select';

const routing: Routes = [
    {
        path: '', data: { name: '' }, children: [
            { path: '', component: ARPrePaidPaymentComponent },
        ]
    },

];

@NgModule({
    declarations: [
        ARPrePaidPaymentComponent,
        ARPrePaidPaymentFormSearchComponent
    ],
    imports: [
        SharedModule,
        RouterModule.forChild(routing),
        TabsModule.forRoot(),
        ModalModule.forRoot(),
        NgSelectModule
    ],
    exports: [],
    providers: [],
})
export class ARPrepaidPaymentModule { }
