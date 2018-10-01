import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { LocationComponent } from './location/location.component';
import { PartnerComponent } from './partner-data/partner.component';
import { WarehouseComponent } from './warehouse/warehouse.component';
import { ChargeComponent } from './charge/charge.component';
import { CommodityComponent } from './commodity/commodity.component';
import { PortIndexComponent } from './port-index/port-index.component';
import { StageManagementComponent } from './stage-management/stage-management.component';
import { UnitComponent } from './unit/unit.component';

const routes: Routes = [

  {
    path:'',
    redirectTo:'location',
    pathMatch:'full'
  },
  {
    path:'location',
    component:LocationComponent
  },
  {
    path:'ware-house',
    component:WarehouseComponent
  },
  {
    path:'charge',
    component:ChargeComponent
  },
  {
    path:'commodity',
    component:CommodityComponent
  },
  {
    path:'partner-data',
    component:PartnerComponent
  },
  {
    path:'port-index',
    component:PortIndexComponent
  },
  {
    path:'stage-management',
    component:StageManagementComponent
  },
  {
    path:'unit',
    component:UnitComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CatalogueRoutingModule { }
