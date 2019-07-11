import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { PLReportComponent } from './p-l-report/p-l-report.component';
import { PerformanceReportComponent } from './performance-report/performance-report.component';
import { ShipmentOverviewComponent } from './shipment-overview/shipment-overview.component';

const routes: Routes = [
  {
    path:'',
    redirectTo:'pl-report'
  },
  {
    path:'pl-report',
    component:PLReportComponent
  },
  {
    path:'performance-report',
    component:PerformanceReportComponent
  },
  {
    path:'shipment-overview',
    component:ShipmentOverviewComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ReportRoutingModule { }
