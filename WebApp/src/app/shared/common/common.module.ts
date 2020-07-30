import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SubHeaderComponent, InputTableLayoutComponent, BreadcrumbComponent, DefaultButtonComponent, SearchOptionsComponent, ReportPreviewComponent, ComboGridVirtualScrollComponent, AppMultipleSelectComponent, TableNoneRecordComponent, TableHeaderComponent, TableRowLoadingComponent, TableCollapseRowComponent, AppTableComponent, AppComboGridComponent, SwitchToggleComponent, TableBodyComponent, AppPermissionButtonComponent, ExportCrystalComponent, AppComboGridIconComponent } from '.';
import { ModalModule, CollapseModule, TooltipModule, PaginationModule, BsDropdownModule } from 'ngx-bootstrap';
import { ConfirmPopupComponent, InfoPopupComponent, Permission403PopupComponent } from './popup';
import { SelectModule } from 'ng2-select';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { DirectiveModule } from '../directives/directive.module';
import { FormsModule } from '@angular/forms';
import { AppPaginationComponent } from './pagination/pagination.component';
import { TableCellComponent } from './table-cell/table-cell.component';
import { PipeModule } from '../pipes/pipe.module';
import { RouterModule } from '@angular/router';
import { AppCombogridItemComponent } from './combo-grid-virtual-scroll/combogrid-item/combo-grid-item.component';
const COMPONENTS = [
    SubHeaderComponent,
    InputTableLayoutComponent,
    BreadcrumbComponent,
    DefaultButtonComponent,
    AppPaginationComponent,
    SearchOptionsComponent,
    ReportPreviewComponent,
    ComboGridVirtualScrollComponent,
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
    TableCellComponent,
    AppPermissionButtonComponent,
    ExportCrystalComponent,
    Permission403PopupComponent,
    AppComboGridIconComponent,
    AppCombogridItemComponent
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
