import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DesignModulesRoutingModule } from './design-modules-routing.module';
import { FormComponent } from './form/form.component';
import { TableComponent } from './table/table.component';
import { SharedModule } from '../shared/shared.module';
import { SelectModule } from 'ng2-select';
import { Daterangepicker } from 'ng2-daterangepicker';

@NgModule({
  imports: [
    CommonModule,
    DesignModulesRoutingModule,
    SharedModule,
    Daterangepicker,
    SelectModule
  ],
  declarations: [FormComponent, TableComponent]
})
export class DesignModulesModule { }
