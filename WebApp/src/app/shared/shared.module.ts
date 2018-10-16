import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SharedRoutingModule } from './shared-routing.module';
import { BreadcrumbComponent } from './breadcrumb/breadcrumb.component';
import { PagingClientComponent } from './paging-client/paging-client.component';

@NgModule({
  imports: [
    CommonModule,
    SharedRoutingModule
  ],
  declarations: [
    BreadcrumbComponent,
    PagingClientComponent
  ],
  exports:[BreadcrumbComponent,PagingClientComponent]

})
export class SharedModule { }
