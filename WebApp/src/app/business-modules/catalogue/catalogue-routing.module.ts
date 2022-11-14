import { NgModule } from "@angular/core";
import { Routes, RouterModule } from '@angular/router';
import { MenuResolveGuard } from "@core";

const routes: Routes = [
  { path: '', redirectTo: 'ware-house' },
  {
    path: 'ware-house', loadChildren: () => import('./warehouse/warehouse.module').then(m => m.WareHouseModule), data: { name: 'Warehouse' },
    resolve: {
      checkMenu: MenuResolveGuard
    },
  },
  {
    path: 'port-index', loadChildren: () => import('./port-index/port-index.module').then(m => m.PortIndexModule), data: { name: 'Port' },
    resolve: {
      checkMenu: MenuResolveGuard
    },
  },
  {
    path: 'partner-data', loadChildren: () => import('./partner-data/partner-data.module').then(m => m.PartnerDataModule), data: { name: 'Supplier/Carrier' },
    resolve: {
      checkMenu: MenuResolveGuard
    },
  },
  {
    path: 'commodity', loadChildren: () => import('./commodity/commodity.module').then(m => m.CommondityModule), data: { name: 'Commodity' },
    resolve: {
      checkMenu: MenuResolveGuard
    },
  },
  {
    path: 'stage-management', loadChildren: () => import('./stage-management/stage-management.module').then(m => m.StateManagementModule), data: { name: 'Stage' },
    resolve: {
      checkMenu: MenuResolveGuard
    },
  },
  {
    path: 'unit', loadChildren: () => import('./unit/unit.module').then(m => m.UnitModule), data: { name: 'Unit' },
    resolve: {
      checkMenu: MenuResolveGuard
    },
  },
  {
    path: 'location', loadChildren: () => import('./location/location.module').then(m => m.LocationModule), data: { name: 'Location' },
    resolve: {
      checkMenu: MenuResolveGuard
    },
  },
  {
    path: 'charge', loadChildren: () => import('./charge/charge.module').then(m => m.ChargeModule), data: { name: 'Charge' },
    resolve: {
      checkMenu: MenuResolveGuard
    },
  },
  {
    path: 'currency', loadChildren: () => import('./currency/currency.module').then(m => m.CurrencyModule), data: { name: 'Currency' },
    resolve: {
      checkMenu: MenuResolveGuard
    },
  },
  {
    path: 'bank', loadChildren: () => import('./bank/bank.module').then(m => m.BankModule), data: { name: 'Bank' },
    resolve: {
      checkMenu: MenuResolveGuard
    },
  },
  {
    path: 'chart-of-accounts', loadChildren: () => import('./chart-of-accounts/chart-of-accounts.module').then(m => m.ChartOfAccountsModule), data: { name: 'Chart Of Accounts' },
    resolve: {
      checkMenu: MenuResolveGuard
    },
  }

  // TODO other module...
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CatalogueRoutingModule { }
