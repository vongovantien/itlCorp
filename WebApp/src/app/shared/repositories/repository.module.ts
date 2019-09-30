import { NgModule } from '@angular/core';
import { CatalogueRepo, AccountingRepo, OperationRepo, SystemRepo, DocumentationRepo, ExportRepo } from '.';

@NgModule({
    providers: [
        SystemRepo,
        AccountingRepo,
        OperationRepo,
        CatalogueRepo,
        DocumentationRepo,
        ExportRepo,
    ],
})
export class RepositoryModule {
}
