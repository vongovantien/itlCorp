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
import { DecimalNumberGreaterThan0Directive, StyleCellDirective, AutofocusDirective, TwoDigitDecimaNumberDirective, ThreeDigitDecimaNumberDirective, IntergerInputDirective, SpecialCharacterDirective, EcusSpecicalCharacterAllowSpaceDirective, EcusSpecicalCharacterNoSpaceDirective, ClickOutSideDirective, AppLoadingButtonDirective, AppRequiredDirective } from "./directives";

import { AppPaginationComponent, TableLayoutComponent, InputTableLayoutComponent, BreadcrumbComponent, DefaultButtonComponent, DeleteConfirmModalComponent, SearchOptionsComponent, InputFormComponent, TableDetailComponent, CloseModalButtonComponent, ReportPreviewComponent, ComboGridVirtualScrollComponent, CfBeforeLeaveModalComponent, AppMultipleSelectComponent, TableNoneRecordComponent, TableHeaderComponent, TableRowLoadingComponent, SubHeaderComponent, TableCollapseRowComponent, AppTableComponent, AppComboGridComponent, SwitchToggleComponent } from "./common";
import { CollapseModule } from "ngx-bootstrap/collapse";
import { UploadAlertComponent } from './common/popup/upload-alert/upload-alert.component';
import { TooltipModule } from "ngx-bootstrap/tooltip";
import { LoadModuleDirective } from "./directives/load-module.directive";
import { TableBodyComponent } from "./common/table-body/table-body.component";
import { CommonComponentModule } from "./common/common.module";
import { DirectiveModule } from "./directives/directive.module";
import { PipeModule } from "./pipes/pipe.module";

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
  AppLoadingButtonDirective,
  AppRequiredDirective
];

const APP_MODULES = [
  DirectiveModule,
  CommonComponentModule,
  PipeModule,

];
@NgModule({
  imports: [
    CommonModule,
    RepositoryModule,
    FormsModule,
    ...APP_MODULES
    // ...Libary
  ],
  declarations: [
    // ...APP_COMPONENTS,
    // ...APP_PIPES,
    // ...APP_DIRECTIVES
  ],
  exports: [
    ...APP_MODULES
    // ...APP_PIPES,
    // ...APP_DIRECTIVES,
    // ...APP_COMPONENTS
  ],
  providers: [API_MENU]
})
export class SharedModule { }
