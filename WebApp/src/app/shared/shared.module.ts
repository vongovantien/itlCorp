import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";

import { ScrollingModule } from "@angular/cdk/scrolling";
import { SelectModule } from "ng2-select";
import { ModalModule } from "ngx-bootstrap/modal";

import { API_MENU } from "../../constants/api-menu.const";
import { RepositoryModule } from "./repositories/repository.module";
import { ServiceModule } from "./services/service.module";
import { SearchStage, FilterPipe } from "./pipes";

import { InfoPopupComponent, ConfirmPopupComponent } from "./common/popup";
import { DecimalNumberGreaterThan0Directive, StyleCellDirective, AutofocusDirective, TwoDigitDecimaNumberDirective, ThreeDigitDecimaNumberDirective, IntergerInputDirective, SpecialCharacterDirective, EcusSpecicalCharacterAllowSpaceDirective, EcusSpecicalCharacterNoSpaceDirective } from "./directives";

import { PaginationComponent, TableLayoutComponent, InputTableLayoutComponent, BreadcrumbComponent, DefaultButtonComponent, DeleteConfirmModalComponent, SearchOptionsComponent, InputFormComponent, TableDetailComponent, CloseModalButtonComponent, ReportPreviewComponent, ComboGridVirtualScrollComponent, CfBeforeLeaveModalComponent } from "./common";

const Libary = [
  ModalModule,
  SelectModule,
  ScrollingModule,
];

const APP_PIPES = [
  SearchStage,
  FilterPipe
];

const APP_POPUP = [
  ConfirmPopupComponent,
  InfoPopupComponent
];

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
