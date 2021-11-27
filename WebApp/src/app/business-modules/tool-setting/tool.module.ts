import { NgModule } from '@angular/core';

import { ToolRoutingModule } from './tool-routing.module';
import { IDDefinitionComponent } from './id-definition/id-definition.component';
import { KPIComponent } from './kpi/kpi.component';
import { SupplierComponent } from './supplier/supplier.component';
import { SharedModule } from '../../shared/shared.module';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { LogViewerComponent } from './log-viewer/log-viewer.component';
import { LinkFeeComponent } from './link-fee/link-fee.component';
import { FormAddRuleComponent } from './link-fee/components/form-add-rule/form-add-rule.component';
import { FormSearchRuleComponent } from './link-fee/components/form-search-rule/form-search-rule.component';
@NgModule({
  imports: [
    ToolRoutingModule,
    SharedModule,
    NgxDaterangepickerMd,
  ],
  declarations: [IDDefinitionComponent, KPIComponent, SupplierComponent, LogViewerComponent, LinkFeeComponent, FormAddRuleComponent, FormSearchRuleComponent]
})
export class ToolModule { }
