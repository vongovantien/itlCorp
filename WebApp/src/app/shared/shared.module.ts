import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";

import { ScrollingModule } from "@angular/cdk/scrolling";
import { SelectModule } from "ng2-select";
import { ModalModule } from "ngx-bootstrap/modal";

import { API_MENU } from "../../constants/api-menu.const";
import { RepositoryModule } from "./repositories/repository.module";
import { ServiceModule } from "./services/service.module";
import { SearchStage, FilterPipe, NegativeNumberePipe, EqualErrorPipe, AbsPipe, RemoveTrimPipe } from "./pipes";

import { InfoPopupComponent, ConfirmPopupComponent } from "./common/popup";
import { DecimalNumberGreaterThan0Directive, StyleCellDirective, AutofocusDirective, TwoDigitDecimaNumberDirective, ThreeDigitDecimaNumberDirective, IntergerInputDirective, SpecialCharacterDirective, EcusSpecicalCharacterAllowSpaceDirective, EcusSpecicalCharacterNoSpaceDirective, ClickOutSideDirective, AppLoadingButtonDirective } from "./directives";

import { PaginationComponent, TableLayoutComponent, InputTableLayoutComponent, BreadcrumbComponent, DefaultButtonComponent, DeleteConfirmModalComponent, SearchOptionsComponent, InputFormComponent, TableDetailComponent, CloseModalButtonComponent, ReportPreviewComponent, ComboGridVirtualScrollComponent, CfBeforeLeaveModalComponent, AppMultipleSelectComponent, TableNoneRecordComponent, TableHeaderComponent, TableRowLoadingComponent, SubHeaderComponent, TableCollapseRowComponent, AppTableComponent } from "./common";
import { CollapseModule } from "ngx-bootstrap/collapse";
import { UploadAlertComponent } from './common/popup/upload-alert/upload-alert.component';
import { TooltipModule } from "ngx-bootstrap/tooltip";
import { LoadModuleDirective } from "./directives/load-module.directive";

const Libary = [
  ModalModule,
  SelectModule,
  ScrollingModule,
  CollapseModule,
  TooltipModule.forRoot()
];

const APP_PIPES = [
  SearchStage,
  FilterPipe,
  NegativeNumberePipe,
  EqualErrorPipe,
  AbsPipe,
  RemoveTrimPipe
];

const APP_POPUP = [
  ConfirmPopupComponent,
  InfoPopupComponent,
  UploadAlertComponent
];

const APP_COMPONENTS = [
  SubHeaderComponent,
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
  AppMultipleSelectComponent,
  TableNoneRecordComponent,
  TableHeaderComponent,
  TableRowLoadingComponent,
  TableCollapseRowComponent,
  AppTableComponent,
  ...APP_POPUP
];

const APP_DIRECTIVES = [
  StyleCellDirective,
  AutofocusDirective,
  TwoDigitDecimaNumberDirective,
  ThreeDigitDecimaNumberDirective,
  IntergerInputDirective,
  SpecialCharacterDirective,
  EcusSpecicalCharacterAllowSpaceDirective,
  EcusSpecicalCharacterNoSpaceDirective,
  DecimalNumberGreaterThan0Directive,
  ClickOutSideDirective,
  LoadModuleDirective,
  AppLoadingButtonDirective
];
@NgModule({
  imports: [
    CommonModule,
    RepositoryModule,
    ServiceModule,
    FormsModule,
    ...Libary
  ],
  declarations: [
    ...APP_COMPONENTS,
    ...APP_PIPES,
    ...APP_DIRECTIVES
  ],
  exports: [
    ...APP_PIPES,
    ...APP_DIRECTIVES,
    ...APP_COMPONENTS
  ],
  providers: [API_MENU]
})
export class SharedModule { }
