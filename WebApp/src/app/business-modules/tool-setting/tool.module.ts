import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ToolRoutingModule } from './tool-routing.module';
import { IDDefinitionComponent } from './id-definition/id-definition.component';
import { TariffComponent } from './tariff/tariff.component';
import { EcusConnectionComponent } from './ecus-connection/ecus-connection.component';
import { KPIComponent } from './kpi/kpi.component';
import { SupplierComponent } from './supplier/supplier.component';
import { SharedModule } from '../../shared/shared.module';
import { ExchangeRateComponent } from './exchange-rate/exchange-rate.component';
import { SelectModule } from 'ng2-select';
import { FormsModule } from '@angular/forms';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';

@NgModule({
  imports: [
    CommonModule,
    ToolRoutingModule,
    SharedModule,
    SelectModule,
    FormsModule, 
    NgxDaterangepickerMd
  ],
  declarations: [IDDefinitionComponent, TariffComponent, EcusConnectionComponent, KPIComponent, SupplierComponent, ExchangeRateComponent]
})
export class ToolModule { }
