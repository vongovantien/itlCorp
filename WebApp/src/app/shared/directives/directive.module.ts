import { NgModule } from '@angular/core';
import {
    AutofocusDirective, TwoDigitDecimaNumberDirective, IntergerInputDirective, SpecialCharacterDirective, DecimalNumberGreaterThan0Directive, ClickOutSideDirective, AppLoadingButtonDirective, AppRequiredDirective, NumericDirective, InjectViewContainerRefDirective, NoDblClickDirective, AutoFormatCurrencyDirective, IconCalendarDirective,
    IConClearCalendarDirective, LoadModuleDirective, DisabledControlDirective, HasOwnerPermissionDirective, ClickStopPropagationDirective, FormatDecimalFormControlDirective, DropdownToggleDirective, ContextMenuDirective, AutoExpandDirective
} from '.';
import { OverlayModule } from '@angular/cdk/overlay';
import { WindowRef } from './windowRef';

const APP_DIRECTIVES = [
    AutofocusDirective,
    TwoDigitDecimaNumberDirective,
    IntergerInputDirective,
    SpecialCharacterDirective,
    DecimalNumberGreaterThan0Directive,
    ClickOutSideDirective,
    LoadModuleDirective,
    AppLoadingButtonDirective,
    AppRequiredDirective,
    NumericDirective,
    AutoFormatCurrencyDirective,
    NoDblClickDirective,
    InjectViewContainerRefDirective,
    IconCalendarDirective,
    IConClearCalendarDirective,
    DisabledControlDirective,
    HasOwnerPermissionDirective,
    ClickStopPropagationDirective,
    FormatDecimalFormControlDirective,
    DropdownToggleDirective,
    ContextMenuDirective,
    AutoExpandDirective
];
@NgModule({
    declarations: [
        ...APP_DIRECTIVES
    ],
    imports: [
        OverlayModule
    ],
    exports: [
        ...APP_DIRECTIVES
    ],
    providers: [
        WindowRef
    ],
})

export class DirectiveModule {

}


