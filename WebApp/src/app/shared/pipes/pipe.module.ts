import { NgModule } from '@angular/core';
import { SearchStage, FilterPipe, NegativeNumberePipe, EqualErrorPipe, AbsPipe, RemoveTrimPipe, HighlightPipe, ServiceNamePipe, SpecialPermissionPipe, SumPipe, DateAgoPipe, SafePipe, ClassStatusApprovalPipe, ExtensionPipe, ClassStatusSyncPipe } from '.';

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
    SafePipe,
    ClassStatusApprovalPipe,
    ExtensionPipe,
    ClassStatusSyncPipe
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