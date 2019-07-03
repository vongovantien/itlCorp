import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { TableLayoutComponent } from "./common/table-layout/table-layout.component";
import { StyleCellDirective } from "./directives/style-cell.directive";
import { InputTableLayoutComponent } from "./common/input-table-layout/input-table-layout.component";
import { BreadcrumbComponent } from "./common/breadcrumb/breadcrumb.component";
import { DefaultButtonComponent } from "./common/default-button/default-button.component";
import { PaginationComponent } from "./common/pagination/pagination.component";
import { SelectModule } from "ng2-select";
import { SortService } from "./services/sort.service";
import { DeleteConfirmModalComponent } from "./common/delete-confirm-modal/delete-confirm-modal.component";
import { API_MENU } from "../../constants/api-menu.const";
import { SearchOptionsComponent } from "./common/search-options/search-options.component";
import { InputFormComponent } from "./common/input-form/input-form.component";
import { TableDetailComponent } from "./common/table-detail/table-detail.component";
import { CloseModalButtonComponent } from "./common/close-modal-button/close-modal-button.component";
import { ExcelService } from "./services/excel.service";
import { NgProgressModule } from "@ngx-progressbar/core";
import { TwoDigitDecimaNumberDirective } from "./directives/two-digit-decima-number.directive";
import { ThreeDigitDecimaNumberDirective } from "./directives/three-digit-decima-number.directive";
import { IntergerInputDirective } from "./directives/interger-input.directive";
import { ReportPreviewComponent } from "./common/report-preview/report-preview.component";
import { SpecialCharacterDirective } from "./directives/specialChracter.directive";
import { EcusSpecicalCharacterAllowSpaceDirective } from "./directives/ecusSpecicalCharacterAllowSpace.directive";
import { EcusSpecicalCharacterNoSpaceDirective } from "./directives/ecusSpecicalCharacterNoSpace.directive";
import { ScrollingModule } from "@angular/cdk/scrolling";
import { ComboGridVirtualScrollComponent } from "./common/combo-grid-virtual-scroll/combo-grid-virtual-scroll.component";
import { RouterModule } from "@angular/router";
import { CfBeforeLeaveModalComponent } from "./common/cf-before-leave-modal/cf-before-leave-modal.component";
import { AutofocusDirective } from "../shared/directives/auto-focus.directive";
import { ModalModule } from "ngx-bootstrap";

const Libary = [ModalModule.forRoot()];
@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    SelectModule,
    ScrollingModule,
    RouterModule,
    ...Libary
  ],
  declarations: [
    TableLayoutComponent,
    StyleCellDirective,
    InputTableLayoutComponent,
    BreadcrumbComponent,
    DefaultButtonComponent,
    PaginationComponent,
    DeleteConfirmModalComponent,
    SearchOptionsComponent,
    InputFormComponent,
    TableDetailComponent,
    CloseModalButtonComponent,
    TwoDigitDecimaNumberDirective,
    ThreeDigitDecimaNumberDirective,
    IntergerInputDirective,
    ReportPreviewComponent,
    SpecialCharacterDirective,
    EcusSpecicalCharacterAllowSpaceDirective,
    EcusSpecicalCharacterNoSpaceDirective,
    ComboGridVirtualScrollComponent,
    CfBeforeLeaveModalComponent,
    AutofocusDirective
  ],
  exports: [
    CommonModule,
    ScrollingModule,
    TwoDigitDecimaNumberDirective,
    ThreeDigitDecimaNumberDirective,
    IntergerInputDirective,
    TableLayoutComponent,
    BreadcrumbComponent,
    DefaultButtonComponent,
    PaginationComponent,
    DeleteConfirmModalComponent,
    SearchOptionsComponent,
    InputFormComponent,
    TableDetailComponent,
    CloseModalButtonComponent,
    NgProgressModule,
    ReportPreviewComponent,
    SpecialCharacterDirective,
    EcusSpecicalCharacterAllowSpaceDirective,
    EcusSpecicalCharacterNoSpaceDirective,
    ComboGridVirtualScrollComponent,
    CfBeforeLeaveModalComponent,
    AutofocusDirective
  ],
  providers: [SortService, ExcelService, API_MENU]
})
export class SharedModule {}
