import { NgModule } from '@angular/core';

import { ChargeComponent } from './charge.component';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'ng2-select';
import { NgProgressModule } from '@ngx-progressbar/core';
import { SharedModule } from 'src/app/shared/shared.module';
import { Routes, RouteConfigLoadEnd, RouterModule } from '@angular/router';
import { ChargeAddnewComponent } from '../charge-addnew/charge-addnew.component';
import { ChargeDetailsComponent } from '../charge-details/charge-details.component';
import { ChargeImportComponent } from '../charge-import/charge-import.component';
import { ChargeImportAccountVoucherComponent } from '../charge-import-account-voucher/charge-import-account-voucher.component';

const routing: Routes = [
    { path: '', component: ChargeComponent, data: { name: "Charge", level: 2 } },
    { path: 'addnew', component: ChargeAddnewComponent, data: { name: "Addnew Charge", level: 3 } },
    { path: 'edit', component: ChargeDetailsComponent, data: { name: "Edit Charge", level: 3 } },
    { path: 'import', component: ChargeImportComponent, data: { name: "Edit Import", level: 3 } },
    { path: 'import-account-voucher', component: ChargeImportAccountVoucherComponent },
];
@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        SelectModule,
        NgProgressModule,
        SharedModule,
        RouterModule.forChild(routing)
    ],
    exports: [],
    declarations: [
        ChargeComponent,
        ChargeAddnewComponent,
        ChargeDetailsComponent,
        ChargeImportComponent,
        ChargeImportAccountVoucherComponent
    ],
    providers: [],
})
export class ChargeModule { }
