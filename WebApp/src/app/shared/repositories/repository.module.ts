import { NgModule } from '@angular/core';
import { JobRepo, SystemRepo, CDNoteRepo, AccoutingRepo, UnitRepo, CustomDeclarationRepo } from '.';
import { ContainerRepo } from './container.repo';

@NgModule({
    providers: [
        JobRepo,
        SystemRepo,
        CDNoteRepo,
        AccoutingRepo,
        ContainerRepo,
        UnitRepo,
        CustomDeclarationRepo
    ],
})
export class RepositoryModule {
}