import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableLayoutComponent } from './common/table-layout/table-layout.component';
import { StyleCellDirective } from './directives/style-cell.directive';
import { SortService } from './services/sort.service';
import { InputTableLayoutComponent } from './common/input-table-layout/input-table-layout.component';
import { BreadcrumbComponent } from './common/breadcrumb/breadcrumb.component';
import { AddmodalButtonComponent } from './common/addmodal-button/addmodal-button.component';

@NgModule({
    imports: [ CommonModule ],
    declarations: [ TableLayoutComponent, StyleCellDirective, InputTableLayoutComponent, BreadcrumbComponent, AddmodalButtonComponent ],
    exports: [
        CommonModule, 
        TableLayoutComponent,
        BreadcrumbComponent,
        AddmodalButtonComponent
    ],
    providers: [
        SortService
    ]
})
export class SharedModule { }