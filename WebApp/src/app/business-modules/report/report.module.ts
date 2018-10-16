import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ReportRoutingModule } from './report-routing.module';
import { PerformanceReportComponent } from './performance-report/performance-report.component';
import { ShipmentOverviewComponent } from './shipment-overview/shipment-overview.component';
import { PLReportComponent } from './p-l-report/p-l-report.component';
import { SharedModule } from '../../shared/shared.module';

@NgModule({
  imports: [
    CommonModule,
    ReportRoutingModule,
    SharedModule
  ],
  declarations: [PerformanceReportComponent, ShipmentOverviewComponent, PLReportComponent]
})
export class ReportModule { }
