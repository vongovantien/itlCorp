import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { MenuResolveGuard } from '@core';
import { PLReportComponent } from './p-l-report/p-l-report.component';
import { PerformanceReportComponent } from './performance-report/performance-report.component';
import { ShipmentOverviewComponent } from './shipment-overview/shipment-overview.component';

const routes: Routes = [
  {
    path: '',
    redirectTo: 'general-report'
  },
  {
    path: 'pl-report',
    component: PLReportComponent
  },
  {
    path: 'performance-report',
    component: PerformanceReportComponent
  },
  {
    path: 'shipment-overview',
    component: ShipmentOverviewComponent
  },
  {
    path: 'general-report', loadChildren: () => import('./general-report/general-report.module').then(m => m.GeneralReportModule),
    data: { name: 'General Report', title: "General Report" },
    resolve: {
      checkMenu: MenuResolveGuard
    },
  },
  {
    path: 'sale-report', loadChildren: () => import('./sale-report/sale-report.module').then(m => m.SaleReportModule),
    data: { name: 'Sale Report', title: "Sale Report" },
    resolve: {
      checkMenu: MenuResolveGuard
    },
  },
  {
    path: 'sheet-debit-report', loadChildren: () => import('./sheet-debit-report/sheet-debit-report.module').then(m => m.SheetDebitReportModule),
    data: { name: 'Accountant Report', title: "Accountant Report" },
    resolve: {
      checkMenu: MenuResolveGuard
    },
  },
  {
    path: 'commission-incentive-report', loadChildren: () => import('./commission-incentive-report/commission-incentive-report.module').then(m => m.CommissionIncentiveReportModule),
    data: { name: 'Commission/ Incentive', title: "Commission/Incentive Report" },
    resolve: {
      checkMenu: MenuResolveGuard
    },
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ReportRoutingModule { }
