import { NgModule } from '@angular/core';
import { SearchStage, FilterPipe, NegativeNumberePipe, EqualErrorPipe, AbsPipe, RemoveTrimPipe, HighlightPipe, ServiceNamePipe, SpecialPermissionPipe, SumPipe, DateAgoPipe, SafePipe, ClassStatusApprovalPipe, ExtensionPipe, ClassStatusSyncPipe, SortTableClassPipe } from '.';

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
    ClassStatusSyncPipe,
    SortTableClassPipe
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