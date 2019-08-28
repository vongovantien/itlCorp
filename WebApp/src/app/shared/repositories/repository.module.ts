import { NgModule } from '@angular/core';
import { JobRepo, SystemRepo, CDNoteRepo, AccoutingRepo, UnitRepo } from '.';
import { OperationRepo } from './operation.repo';

@NgModule({
    providers: [
        JobRepo,
        SystemRepo,
        CDNoteRepo,
        AccoutingRepo,
        UnitRepo,
        OperationRepo
    ],
})
export class RepositoryModule {
}