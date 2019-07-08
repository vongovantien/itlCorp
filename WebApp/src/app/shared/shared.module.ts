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
import { RepositoryModule } from "./repositories/repository.module";
import { ServiceModule } from "./services/service.module";
import { SearchStage } from "./pipes";

const Libary = [ModalModule.forRoot()];

const APP_PIPES = [
  SearchStage
]

const APP_COMPONENTS = [
  TableLayoutComponent,
  InputTableLayoutComponent,
  BreadcrumbComponent,
  DefaultButtonComponent,
  PaginationComponent,
  DeleteConfirmModalComponent,
  SearchOptionsComponent,
  InputFormComponent,
  TableDetailComponent,
  CloseModalButtonComponent,
  ReportPreviewComponent,
  ComboGridVirtualScrollComponent,
  CfBeforeLeaveModalComponent,
]

const APP_DIRECTIVES = [
  StyleCellDirective,
  AutofocusDirective,
  TwoDigitDecimaNumberDirective,
  ThreeDigitDecimaNumberDirective,
  IntergerInputDirective,
  SpecialCharacterDirective,
  EcusSpecicalCharacterAllowSpaceDirective,
  EcusSpecicalCharacterNoSpaceDirective,

]
@NgModule({
  imports: [
    CommonModule,
    RepositoryModule,
    ServiceModule,
    FormsModule,
    SelectModule,
    ScrollingModule,
    RouterModule,
    ...Libary
  ],
  declarations: [
    ...APP_COMPONENTS,
    ...APP_PIPES,
    ...APP_DIRECTIVES
  ],
  exports: [
    ScrollingModule,
    NgProgressModule,
    ...APP_PIPES,
    ...APP_DIRECTIVES,
    ...APP_COMPONENTS
  ],
  providers: [API_MENU]
})
export class SharedModule { }
