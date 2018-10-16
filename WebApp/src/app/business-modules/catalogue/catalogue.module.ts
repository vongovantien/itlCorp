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



@NgModule({
  imports: [
    CommonModule,
    CatalogueRoutingModule,
    FormsModule,
    SharedModule
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
      
      ],
})
export class CatalogueModule { }
