import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '../../shared/shared.module';
import { CatalogueRoutingModule } from './catalogue-routing.module';
import { LocationComponent } from './location/location.component';
import { WarehouseComponent } from './warehouse/warehouse.component';
import { PortIndexComponent } from './port-index/port-index.component';
import { CommodityComponent } from './commodity/commodity.component';
import { ChargeComponent } from './charge/charge.component';
import { UnitComponent } from './unit/unit.component';
import { StageManagementComponent } from './stage-management/stage-management.component';
import { PartnerComponent } from './partner-data/partner.component';
import { ModalModule } from 'ngx-bootstrap';
import { SelectModule } from 'ng2-select';
import { FormsModule} from '@angular/forms';
import { PartnerDataAddnewComponent } from './partner-data-addnew/partner-data-addnew.component';
import { PartnerDataDetailComponent } from './partner-data-detail/partner-data-detail.component';
import { ChargeAddnewComponent } from './charge-addnew/charge-addnew.component';
import { CustomerComponent } from './partner-data/customer/customer.component';
import { AgentComponent } from './partner-data/agent/agent.component';
import { CarrierComponent } from './partner-data/carrier/carrier.component';
import { ConsigneeComponent } from './partner-data/consignee/consignee.component';
import { AirShipSupComponent } from './partner-data/air-ship-sup/air-ship-sup.component';
import { ShipperComponent } from './partner-data/shipper/shipper.component';
import { AllPartnerComponent } from './partner-data/all/all-partner.component';
import { CurrencyComponent } from './currency/currency.component';
import { ChargeDetailsComponent } from './charge-details/charge-details.component';
import { WarehouseImportComponent } from './warehouse-import/warehouse-import.component';


@NgModule({
  imports: [
    CommonModule,
    CatalogueRoutingModule,
    SharedModule,
    FormsModule,
    SelectModule,
    ModalModule.forRoot()  ],
  declarations:
    [LocationComponent,
      WarehouseComponent,
      PortIndexComponent,
      CommodityComponent,
      ChargeComponent,
      UnitComponent,
      StageManagementComponent,
      PartnerComponent,
      PartnerDataAddnewComponent,
      PartnerDataDetailComponent,
      ChargeAddnewComponent,
      CustomerComponent,
      AgentComponent,
      CarrierComponent,
      ConsigneeComponent,
      AirShipSupComponent,
      ShipperComponent,
 	    CurrencyComponent,
      AllPartnerComponent,
      ChargeDetailsComponent,
      WarehouseImportComponent
    ],
})
export class CatalogueModule { }
