import { NgModule } from '@angular/core';
import { AutofocusDirective, TwoDigitDecimaNumberDirective, IntergerInputDirective, SpecialCharacterDirective, EcusSpecicalCharacterNoSpaceDirective, DecimalNumberGreaterThan0Directive, ClickOutSideDirective, AppLoadingButtonDirective, AppRequiredDirective, NumericDirective, InjectViewContainerRefDirective } from '.';
import { LoadModuleDirective } from './load-module.directive';
import { AutoFormatCurrencyDirective } from './auto-format-currency.directive';
import { CurrencyPipe } from '@angular/common';
import { NoDblClickDirective } from './prevent-dbclick.directive';
import { AutoFormatDecimalDirective } from './number-digit-decimal.directive';

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
    AutoFormatDecimalDirective
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
