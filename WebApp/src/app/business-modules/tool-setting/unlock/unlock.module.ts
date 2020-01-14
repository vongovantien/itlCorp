import { NgModule } from '@angular/core';
import { UnlockComponent } from './unlock.component';
import { Route, RouterModule } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { TabsModule } from 'ngx-bootstrap';
import { UnlockShipmentComponent } from './components/unlock-shipment/unlock-shipment.component';
import { UnlockAccountingComponent } from './components/unlock-accounting/unlock-accouting.component';
import { SelectModule } from 'ng2-select';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

const routing: Route[] = [
    { path: '', component: UnlockComponent, data: { name: "Unlock", level: 2 } },

];

@NgModule({
    imports: [
        CommonModule,
        RouterModule.forChild(routing),
        SharedModule,
        TabsModule.forRoot(),
        SelectModule,
        NgxDaterangepickerMd,
        FormsModule
    ],
    exports: [],
    declarations: [
        UnlockComponent,
        UnlockShipmentComponent,
        UnlockAccountingComponent,
    ],
    providers: [],
})
export class UnlockModule { }
