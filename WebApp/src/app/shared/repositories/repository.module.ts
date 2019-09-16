import { NgModule } from '@angular/core';
import { JobRepo, SystemRepo, CDNoteRepo, AccoutingRepo, UnitRepo, CustomDeclarationRepo } from '.';
import { OperationRepo } from './operation.repo';
import { CatalogueRepo } from './catalogue.repo';

@NgModule({
    providers: [
        JobRepo,
        SystemRepo,
        CDNoteRepo,
        AccoutingRepo,
        UnitRepo,
        OperationRepo,
        CustomDeclarationRepo,
        CatalogueRepo
    ],
})
export class RepositoryModule {
}