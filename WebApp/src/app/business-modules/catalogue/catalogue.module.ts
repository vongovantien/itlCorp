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

import { FormsModule, ReactiveFormsModule, FormControl } from '@angular/forms';
import { PartnerDataAddnewComponent } from './partner-data-addnew/partner-data-addnew.component';
import { SelectModule } from 'ng2-select';
import { PartnerDataDetailComponent } from './partner-data-detail/partner-data-detail.component';


@NgModule({
  imports: [
    CommonModule,
    CatalogueRoutingModule,
    SharedModule,
    FormsModule,
    SelectModule
  ],
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
      PartnerDataDetailComponent
    ],
})
export class CatalogueModule { }
