import { NgModule } from '@angular/core';
import { JobRepo, SystemRepo, CDNoteRepo, AccoutingRepo, UnitRepo, CustomDeclarationRepo } from '.';
import { OperationRepo } from './operation.repo';

@NgModule({
    providers: [
        JobRepo,
        SystemRepo,
        CDNoteRepo,
        AccoutingRepo,
        UnitRepo,
        OperationRepo,
        CustomDeclarationRepo
    ],
})
export class RepositoryModule {
}