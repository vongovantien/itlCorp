import { NgModule } from '@angular/core';
import { StyleCellDirective, AutofocusDirective, TwoDigitDecimaNumberDirective, ThreeDigitDecimaNumberDirective, IntergerInputDirective, SpecialCharacterDirective, EcusSpecicalCharacterAllowSpaceDirective, EcusSpecicalCharacterNoSpaceDirective, DecimalNumberGreaterThan0Directive, ClickOutSideDirective, AppLoadingButtonDirective, AppRequiredDirective } from '.';
import { LoadModuleDirective } from './load-module.directive';

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

    ],
})

export class DirectiveModule {

}
