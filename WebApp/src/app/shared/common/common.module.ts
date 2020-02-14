import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SubHeaderComponent, TableLayoutComponent, InputTableLayoutComponent, BreadcrumbComponent, DefaultButtonComponent, DeleteConfirmModalComponent, SearchOptionsComponent, InputFormComponent, TableDetailComponent, CloseModalButtonComponent, ReportPreviewComponent, ComboGridVirtualScrollComponent, CfBeforeLeaveModalComponent, AppMultipleSelectComponent, TableNoneRecordComponent, TableHeaderComponent, TableRowLoadingComponent, TableCollapseRowComponent, AppTableComponent, AppComboGridComponent, SwitchToggleComponent, TableBodyComponent } from '.';
import { ModalModule, CollapseModule, TooltipModule, PaginationModule, BsDropdownModule } from 'ngx-bootstrap';
import { ConfirmPopupComponent, InfoPopupComponent } from './popup';
import { SelectModule } from 'ng2-select';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { DirectiveModule } from '../directives/directive.module';
import { FormsModule } from '@angular/forms';
import { AppPaginationComponent } from './pagination/pagination.component';
import { TableCellComponent } from './table-cell/table-cell.component';
import { PipeModule } from '../pipes/pipe.module';
import { RouterModule } from '@angular/router';

const COMPONENTS = [
    SubHeaderComponent,
    TableLayoutComponent,
    InputTableLayoutComponent,
    BreadcrumbComponent,
    DefaultButtonComponent,
    AppPaginationComponent,
    DeleteConfirmModalComponent,
    SearchOptionsComponent,
    InputFormComponent,
    TableDetailComponent,
    CloseModalButtonComponent,
    ReportPreviewComponent,
    ComboGridVirtualScrollComponent,
    CfBeforeLeaveModalComponent,
    AppMultipleSelectComponent,
    TableNoneRecordComponent,
    TableHeaderComponent,
    TableRowLoadingComponent,
    TableCollapseRowComponent,
    AppTableComponent,
    AppComboGridComponent,
    SwitchToggleComponent,
    TableBodyComponent,
    ConfirmPopupComponent,
    InfoPopupComponent,
    TableCellComponent

];

@NgModule({
    declarations: [
        ...COMPONENTS
    ],
    imports: [
        RouterModule,
        CommonModule,
        ModalModule,
        SelectModule,
        ScrollingModule,
        PaginationModule.forRoot(),
        CollapseModule,
        FormsModule,
        DirectiveModule,
        PipeModule,
        TooltipModule.forRoot(),
        BsDropdownModule.forRoot()
    ],
    exports: [
        ...COMPONENTS
    ],
    providers: [],
})
export class CommonComponentModule {

}
