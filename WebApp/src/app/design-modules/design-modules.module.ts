import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DesignModulesRoutingModule } from './design-modules-routing.module';
import { FormComponent } from './form/form.component';
import { TableComponent } from './table/table.component';
import { SharedModule } from '../shared/shared.module';
import { SelectModule } from 'ng2-select';
import { FormsModule } from '@angular/forms';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';

@NgModule({
  imports: [
    CommonModule,
    DesignModulesRoutingModule,
    SharedModule,
    NgxDaterangepickerMd,
    FormsModule,
    SelectModule
  ],
  declarations: [FormComponent, TableComponent]
})
export class DesignModulesModule { }
