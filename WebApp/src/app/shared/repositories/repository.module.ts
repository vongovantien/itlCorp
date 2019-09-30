import { NgModule } from '@angular/core';
import { CatalogueRepo, AccoutingRepo, OperationRepo, SystemRepo, DocumentationRepo, ExportRepo } from '.';

@NgModule({
    providers: [
        SystemRepo,
        AccoutingRepo,
        OperationRepo,
        CatalogueRepo,
        DocumentationRepo,
        ExportRepo,
    ],
})
export class RepositoryModule {
}
