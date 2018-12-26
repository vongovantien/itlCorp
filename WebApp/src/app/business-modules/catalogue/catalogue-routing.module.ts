import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { LocationComponent } from './location/location.component';
import { PartnerComponent } from './partner-data/partner.component';
import { WarehouseComponent } from './warehouse/warehouse.component';
import { WarehouseImportComponent } from './warehouse-import/warehouse-import.component';
import { ChargeComponent } from './charge/charge.component';
import { ChargeAddnewComponent } from './charge-addnew/charge-addnew.component';
import { CommodityComponent } from './commodity/commodity.component';
import { PortIndexComponent } from './port-index/port-index.component';
import { PortIndexImportComponent } from './port-index-import/port-index-import.component';
import { StageManagementComponent } from './stage-management/stage-management.component';
import { UnitComponent } from './unit/unit.component';
import { PartnerDataAddnewComponent } from './partner-data-addnew/partner-data-addnew.component';
import { PartnerDataDetailComponent } from './partner-data-detail/partner-data-detail.component';
import { ChargeDetailsComponent } from './charge-details/charge-details.component';
import { CurrencyComponent } from './currency/currency.component';
import { LocationImportComponent } from './location-import/location-import.component';
import { StageImportComponent } from './stage-import/stage-import.component';

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
    path:'location-import',
    component:LocationImportComponent
  },
  {
    path:'ware-house',
    component:WarehouseComponent
  },
  {
    path:'ware-house-import',
    component:WarehouseImportComponent
  },
  {
    path:'charge',
    component:ChargeComponent
  },
  {
    path:'charge-addnew',
    component:ChargeAddnewComponent
  },
  {
    path:'charge-edit',
    component:ChargeDetailsComponent
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
    path:'port-index-import',
    component:PortIndexImportComponent
  },
  {
    path:'stage-management',
    component:StageManagementComponent
  },
  {
    path:'stage-import',
    component:StageImportComponent
  },
  {
    path:'unit',
    component:UnitComponent
  },
  {
    path:'partner-data-addnew',
    component:PartnerDataAddnewComponent
  },
  {
    path:'partner-data-detail',
    component:PartnerDataDetailComponent
  },
  {
    path:'currency',
    component:CurrencyComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CatalogueRoutingModule { }
