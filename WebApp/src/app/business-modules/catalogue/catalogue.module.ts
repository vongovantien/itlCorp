import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { CatalogueRoutingModule } from './catalogue-routing.module';
import { PartnerComponent } from './partner/partner.component';
import { LocationComponent } from './location/location.component';
import { WarehouseComponent } from './warehouse/warehouse.component';

@NgModule({
  imports: [
    CommonModule,
    CatalogueRoutingModule
  ],
  declarations: [PartnerComponent, LocationComponent, WarehouseComponent],

})
export class CatalogueModule { }
