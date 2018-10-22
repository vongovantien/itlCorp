import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ModalModule } from 'ngx-bootstrap';

import { TableLayoutComponent } from './common/table-layout/table-layout.component';
import { StyleCellDirective } from './directives/style-cell.directive';
import { InputTableLayoutComponent } from './common/input-table-layout/input-table-layout.component';
import { BreadcrumbComponent } from './common/breadcrumb/breadcrumb.component';
import { DefaultButtonComponent } from './common/default-button/default-button.component';
import { PagingClientComponent } from 'src/app/shared/paging-client/paging-client.component';
import { PaginationComponent } from './common/pagination/pagination.component';

import { SortService } from './services/sort.service';
import { PagerService } from './services/pager.service';
import { DeleteConfirmModalComponent } from './common/delete-confirm-modal/delete-confirm-modal.component';
import { API_MENU } from '../../constants/api-menu.const';
import { PagingService } from './paging-client/paging-client-service';
import { SearchOptionsComponent } from './common/search-options/search-options.component';
import { ModifiedModalComponent } from './common/modified-modal/modified-modal.component';
import { InputFormComponent } from './common/input-form/input-form.component';

@NgModule({
  imports: [CommonModule, FormsModule, ModalModule],
  declarations: [
    TableLayoutComponent, 
    StyleCellDirective, 
    InputTableLayoutComponent, 
    BreadcrumbComponent, 
    DefaultButtonComponent, 
    PagingClientComponent, 
    PaginationComponent, 
    DeleteConfirmModalComponent, 
    SearchOptionsComponent, 
    ModifiedModalComponent, 
    InputFormComponent
  ],
  exports: [
    CommonModule,
    ModalModule,
    TableLayoutComponent,
    BreadcrumbComponent,
    DefaultButtonComponent,
    PagingClientComponent,
    PaginationComponent,
    DeleteConfirmModalComponent,
    SearchOptionsComponent,
    ModifiedModalComponent,
    InputFormComponent
  ],
  providers: [
    SortService,
    PagerService,
    API_MENU,    
    PagingService
  ]
})
export class SharedModule { }

