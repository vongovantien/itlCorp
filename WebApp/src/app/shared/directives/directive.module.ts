import { NgModule } from '@angular/core';
import {
    AutofocusDirective, TwoDigitDecimaNumberDirective, SpecialCharacterDirective, DecimalNumberGreaterThan0Directive, ClickOutSideDirective, AppLoadingButtonDirective, AppRequiredDirective, NumericDirective, InjectViewContainerRefDirective, NoDblClickDirective, AutoFormatCurrencyDirective, IconCalendarDirective,
    IConClearCalendarDirective, LoadModuleDirective, DisabledControlDirective, HasOwnerPermissionDirective, ClickStopPropagationDirective, FormatDecimalFormControlDirective, IntergerInputDirective
} from '.';
import { CurrencyPipe } from '@angular/common';

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
    FormatDecimalFormControlDirective
];
@NgModule({
    declarations: [
        ...APP_DIRECTIVES
    ],
    imports: [
    ],
    exports: [
        ...APP_DIRECTIVES
    ],
    providers: [
        CurrencyPipe
    ],
})

export class DirectiveModule {

}
