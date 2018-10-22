import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { TableLayoutComponent } from './common/table-layout/table-layout.component';
import { StyleCellDirective } from './directives/style-cell.directive';
import { InputTableLayoutComponent } from './common/input-table-layout/input-table-layout.component';
import { BreadcrumbComponent } from './common/breadcrumb/breadcrumb.component';
import { AddmodalButtonComponent } from './common/addmodal-button/addmodal-button.component';
import { PagingClientComponent } from 'src/app/shared/paging-client/paging-client.component';
import { PaginationComponent } from './common/pagination/pagination.component';

import { SortService } from './services/sort.service';
import { PagerService } from './services/pager.service';
import { DeleteConfirmModalComponent } from './common/delete-confirm-modal/delete-confirm-modal.component';
import { API_MENU } from '../../constants/api-menu.const';
import { PagingService } from './paging-client/paging-client-service';

@NgModule({
  imports: [CommonModule, FormsModule],
  declarations: [
    TableLayoutComponent, 
    StyleCellDirective, 
    InputTableLayoutComponent, 
    BreadcrumbComponent, 
    AddmodalButtonComponent, 
    //BreadcrumbComponent,
    PagingClientComponent, PaginationComponent, DeleteConfirmModalComponent
  ],
  exports: [
    CommonModule,
    TableLayoutComponent,
    BreadcrumbComponent,
    AddmodalButtonComponent,
    // BreadcrumbComponent, 
     PagingClientComponent,
     PaginationComponent,
     DeleteConfirmModalComponent
  ],
  providers: [
    SortService,
    PagerService,
	API_MENU,    PagingService
  ]
})
export class SharedModule { }

