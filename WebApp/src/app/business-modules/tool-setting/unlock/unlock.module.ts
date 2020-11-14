import { NgModule } from '@angular/core';
import { UnlockComponent } from './unlock.component';
import { Route, RouterModule } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { ModalModule } from 'ngx-bootstrap/modal';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { UnlockShipmentComponent } from './components/unlock-shipment/unlock-shipment.component';
import { UnlockAccountingComponent } from './components/unlock-accounting/unlock-accouting.component';
import { SelectModule } from 'ng2-select';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { UnlockHistoryPopupComponent } from './components/unlock-history/unlock-history.popup';

const routing: Route[] = [
    { path: '', component: UnlockComponent, data: { name: "" } },

];

@NgModule({
    imports: [
        RouterModule.forChild(routing),
        SharedModule,
        TabsModule.forRoot(),
        SelectModule,
        NgxDaterangepickerMd,
        ModalModule.forRoot(),
    ],
    exports: [],
    declarations: [
        UnlockComponent,
        UnlockShipmentComponent,
        UnlockAccountingComponent,
        UnlockHistoryPopupComponent
    ],
    providers: [],
})
export class UnlockModule { }
