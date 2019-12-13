import { NgModule } from '@angular/core';

import { PartnerComponent } from './partner.component';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/shared/shared.module';
import { Routes, RouterModule } from '@angular/router';
import { PartnerDataImportComponent } from './import/partner-data-import.component';
import { AllPartnerComponent } from './all/all-partner.component';
import { PartnerDataAddnewComponent } from './addnew/partner-data-addnew.component';
import { PartnerDataDetailComponent } from './detail/partner-data-detail.component';
import { CustomerComponent } from './customer/customer.component';
import { AgentComponent } from './agent/agent.component';
import { CarrierComponent } from './carrier/carrier.component';
import { ConsigneeComponent } from './consignee/consignee.component';
import { AirShipSupComponent } from './air-ship-sup/air-ship-sup.component';
import { ShipperComponent } from './shipper/shipper.component';
import { NgProgressModule } from '@ngx-progressbar/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SelectModule } from 'ng2-select';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SalemanPopupComponent } from './components/saleman-popup.component';
import { ModalModule } from 'ngx-bootstrap';

const routing: Routes = [
    { path: '', component: PartnerComponent, data: { name: "Partner Data", level: 2 } },
    { path: 'import', component: PartnerDataImportComponent, data: { name: "Partner Data Import", level: 3 } },
    { path: 'addnew', component: PartnerDataAddnewComponent, data: { name: "Partner Data Addnew", level: 3 } },
    { path: 'detail/:id', component: PartnerDataDetailComponent, data: { name: "Partner Data Details", level: 3 } },
]
@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        RouterModule.forChild(routing),
        NgProgressModule,
        FormsModule,
        ReactiveFormsModule,
        SelectModule,
        NgxDaterangepickerMd,
        PaginationModule.forRoot(),
        ModalModule.forRoot()
    ],
    exports: [],
    declarations: [
        PartnerComponent,
        PartnerDataImportComponent,
        AllPartnerComponent,
        PartnerDataAddnewComponent,
        PartnerDataDetailComponent,
        CustomerComponent,
        AgentComponent,
        CarrierComponent,
        ConsigneeComponent,
        AirShipSupComponent,
        ShipperComponent,
        SalemanPopupComponent

    ],
    providers: [],
})
export class PartnerDataModule { }
