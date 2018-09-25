import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SalesExecutiveRoutingModule } from './sales-executive-routing.module';
import { SalesExecutiveComponent } from './sales-executive/sales-executive.component';

@NgModule({
  imports: [
    CommonModule,
    SalesExecutiveRoutingModule
  ],
  declarations: [SalesExecutiveComponent]
})
export class SalesExecutiveModule { }
