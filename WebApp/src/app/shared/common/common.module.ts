import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SubHeaderComponent, TableLayoutComponent, InputTableLayoutComponent, BreadcrumbComponent, DefaultButtonComponent, DeleteConfirmModalComponent, SearchOptionsComponent, InputFormComponent, TableDetailComponent, CloseModalButtonComponent, ReportPreviewComponent, ComboGridVirtualScrollComponent, CfBeforeLeaveModalComponent, AppMultipleSelectComponent, TableNoneRecordComponent, TableHeaderComponent, TableRowLoadingComponent, TableCollapseRowComponent, AppTableComponent, AppComboGridComponent, SwitchToggleComponent, TableBodyComponent } from '.';
import { ModalModule, CollapseModule, TooltipModule, PaginationModule } from 'ngx-bootstrap';
import { ConfirmPopupComponent, InfoPopupComponent } from './popup';
import { UploadAlertComponent } from './popup/upload-alert/upload-alert.component';
import { SelectModule } from 'ng2-select';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { DirectiveModule } from '../directives/directive.module';
import { FormsModule } from '@angular/forms';
import { AppPaginationComponent } from './pagination/pagination.component';

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
    UploadAlertComponent

];

@NgModule({
    declarations: [
        ...COMPONENTS
    ],
    imports: [
        CommonModule,
        ModalModule,
        SelectModule,
        ScrollingModule,
        PaginationModule.forRoot(),
        CollapseModule,
        FormsModule,
        DirectiveModule,
        TooltipModule.forRoot()
    ],
    exports: [
        ...COMPONENTS
    ],
    providers: [],
})
export class CommonComponentModule {

}
