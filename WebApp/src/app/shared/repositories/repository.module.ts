import { NgModule } from '@angular/core';
import { CatalogueRepo, AccoutingRepo, UnitRepo, OperationRepo, SystemRepo, DocumentationRepo } from '.';

@NgModule({
    providers: [
        SystemRepo,
        AccoutingRepo,
        UnitRepo,
        OperationRepo,
        CatalogueRepo,
        DocumentationRepo
    ],
})
export class RepositoryModule {
}
