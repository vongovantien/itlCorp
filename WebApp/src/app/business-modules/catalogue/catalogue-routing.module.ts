import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { LocationComponent } from './location/location.component';
import { PartnerComponent } from './partner/partner.component';
import { WarehouseComponent } from './warehouse/warehouse.component';

const routes: Routes = [

  {
    path:'',
    redirectTo:'partner',
    pathMatch:'full'
  },
  {
    path:'location',
    component:LocationComponent
  },
  {
    path:'partner',
    component:PartnerComponent
  },
  {
    path:'ware-house',
    component:WarehouseComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CatalogueRoutingModule { }
