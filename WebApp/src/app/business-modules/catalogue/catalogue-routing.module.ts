import { NgModule } from "@angular/core";
import { Routes, RouterModule } from '@angular/router';

const routes: Routes = [
  { path: '', redirectTo: 'ware-house' },
  { path: 'ware-house', loadChildren: () => import('./warehouse/warehouse.module').then(m => m.WareHouseModule) },
  { path: 'port-index', loadChildren: () => import('./port-index/port-index.module').then(m => m.PortIndexModule) },
  { path: 'partner-data', loadChildren: () => import('./partner-data/partner-data.module').then(m => m.PartnerDataModule) },
  { path: 'commodity', loadChildren: () => import('./commodity/commodity.module').then(m => m.CommondityModule) },
  { path: 'stage-management', loadChildren: () => import('./stage-management/stage-management.module').then(m => m.StateManagementModule) },
  { path: 'stage-management', loadChildren: () => import('./stage-management/stage-management.module').then(m => m.StateManagementModule) },
  { path: 'unit', loadChildren: () => import('./unit/unit.module').then(m => m.UnitModule) },
  { path: 'location', loadChildren: () => import('./location/location.module').then(m => m.LocationModule) },
  { path: 'location', loadChildren: () => import('./location/location.module').then(m => m.LocationModule) },
  { path: 'charge', loadChildren: () => import('./charge/charge.module').then(m => m.ChargeModule) },
  { path: 'currency', loadChildren: () => import('./currency/currency.module').then(m => m.CurrencyModule) },
  
  // TODO other module...
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CatalogueRoutingModule { }
