import { NgModule } from '@angular/core';

import { ChargeComponent } from './charge.component';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'ng2-select';
import { NgProgressModule } from '@ngx-progressbar/core';
import { SharedModule } from 'src/app/shared/shared.module';
import { Routes, RouterModule } from '@angular/router';
import { ChargeImportComponent } from './charge-import/charge-import.component';
import { ChargeImportAccountVoucherComponent } from './charge-import-account-voucher/charge-import-account-voucher.component';
import { PaginationModule } from 'ngx-bootstrap';
import { FormSearchChargeComponent } from './components/form-search-charge/form-search-charge.component';
import { AddChargeComponent } from './add-charge/add-charge.component';
import { DetailChargeComponent } from './detail-charge/detail-charge.component';

const routing: Routes = [
    { path: '', component: ChargeComponent, data: { name: "Charge", level: 2 } },
    { path: 'addnew', component: AddChargeComponent, data: { name: "Addnew Charge", level: 3 } },
    { path: 'import', component: ChargeImportComponent, data: { name: "Import", level: 3 } },
    { path: 'import-account-voucher', component: ChargeImportAccountVoucherComponent },
    { path: ':id', component: DetailChargeComponent, data: { name: "Edit Charge", level: 3 } },

];
@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        SelectModule,
        NgProgressModule,
        SharedModule,
        PaginationModule.forRoot(),
        RouterModule.forChild(routing)
    ],
    exports: [],
    declarations: [
        ChargeComponent,
        ChargeImportComponent,
        ChargeImportAccountVoucherComponent,
        FormSearchChargeComponent,
        AddChargeComponent,
        DetailChargeComponent
    ],
    providers: [],
})
export class ChargeModule { }
