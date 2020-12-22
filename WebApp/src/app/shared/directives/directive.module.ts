import { NgModule } from '@angular/core';
import {
    AutofocusDirective, TwoDigitDecimaNumberDirective, IntergerInputDirective, SpecialCharacterDirective, EcusSpecicalCharacterNoSpaceDirective, DecimalNumberGreaterThan0Directive, ClickOutSideDirective, AppLoadingButtonDirective, AppRequiredDirective, NumericDirective, InjectViewContainerRefDirective, NoDblClickDirective, AutoFormatCurrencyDirective, IconCalendarDirective,
    IConClearCalendarDirective, LoadModuleDirective, DisabledControlDirective
} from '.';
import { CurrencyPipe } from '@angular/common';

const APP_DIRECTIVES = [
    AutofocusDirective,
    TwoDigitDecimaNumberDirective,
    IntergerInputDirective,
    SpecialCharacterDirective,
    EcusSpecicalCharacterNoSpaceDirective,
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
    DisabledControlDirective
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
