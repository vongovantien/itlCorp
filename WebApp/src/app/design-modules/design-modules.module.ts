import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DesignModulesRoutingModule } from './design-modules-routing.module';
import { FormComponent } from './form/form.component';
import { TableComponent } from './table/table.component';

@NgModule({
  imports: [
    CommonModule,
    DesignModulesRoutingModule
  ],
  declarations: [FormComponent, TableComponent]
})
export class DesignModulesModule { }
