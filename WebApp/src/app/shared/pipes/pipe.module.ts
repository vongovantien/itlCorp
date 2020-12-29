import { NgModule } from '@angular/core';
import { SearchStage, FilterPipe, NegativeNumberePipe, EqualErrorPipe, AbsPipe, RemoveTrimPipe, HighlightPipe, ServiceNamePipe, SpecialPermissionPipe, SumPipe, DateAgoPipe, SafePipe } from '.';

const APP_PIPES = [
    SearchStage,
    FilterPipe,
    NegativeNumberePipe,
    EqualErrorPipe,
    AbsPipe,
    RemoveTrimPipe,
    HighlightPipe,
    ServiceNamePipe,
    SpecialPermissionPipe,
    SumPipe,
    DateAgoPipe,
    SafePipe
];

@NgModule({
    declarations: [
        ...APP_PIPES
    ],
    imports: [],
    exports: [
        ...APP_PIPES
    ],
    providers: [],
})
export class PipeModule {

}