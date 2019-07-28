import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DesignModulesRoutingModule } from './design-modules-routing.module';
import { FormComponent } from './form/form.component';
import { TableComponent } from './table/table.component';
import { SharedModule } from '../shared/shared.module';
import { SelectModule } from 'ng2-select';
import { FormsModule } from '@angular/forms';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { PerfectScrollbarModule, PerfectScrollbarConfigInterface, PERFECT_SCROLLBAR_CONFIG } from 'ngx-perfect-scrollbar';

const DEFAULT_PERFECT_SCROLLBAR_CONFIG: PerfectScrollbarConfigInterface = {
  // wheelPropagation: true
};
@NgModule({
  imports: [
    // CommonModule,
    DesignModulesRoutingModule,
    SharedModule,
    NgxDaterangepickerMd,
    FormsModule,
    SelectModule,
    PerfectScrollbarModule, // Scrollbar
  ],
  declarations: [
    FormComponent, 
    TableComponent
  ],
  providers: [
    { 
      provide: PERFECT_SCROLLBAR_CONFIG, // Scrollbar
      useValue: DEFAULT_PERFECT_SCROLLBAR_CONFIG // Scrollbar
    }
  ],
})
export class DesignModulesModule { }
