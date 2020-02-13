import { NgModule } from '@angular/core';
import { StyleCellDirective, AutofocusDirective, TwoDigitDecimaNumberDirective, ThreeDigitDecimaNumberDirective, IntergerInputDirective, SpecialCharacterDirective, EcusSpecicalCharacterAllowSpaceDirective, EcusSpecicalCharacterNoSpaceDirective, DecimalNumberGreaterThan0Directive, ClickOutSideDirective, AppLoadingButtonDirective, AppRequiredDirective, NumericDirective } from '.';
import { LoadModuleDirective } from './load-module.directive';
import { AutoFormatCurrencyDirective } from './auto-format-currency.directive';
import { CurrencyPipe } from '@angular/common';

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
    AppRequiredDirective,
    NumericDirective,
    AutoFormatCurrencyDirective
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
