import { NgModule } from '@angular/core';
import { AutofocusDirective, TwoDigitDecimaNumberDirective, IntergerInputDirective, SpecialCharacterDirective, EcusSpecicalCharacterNoSpaceDirective, DecimalNumberGreaterThan0Directive, ClickOutSideDirective, AppLoadingButtonDirective, AppRequiredDirective, NumericDirective, InjectViewContainerRefDirective, NoDblClickDirective, AutoFormatCurrencyDirective } from '.';
import { CurrencyPipe } from '@angular/common';
import { LoadModuleDirective } from './load-module.directive';

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
    InjectViewContainerRefDirective
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
