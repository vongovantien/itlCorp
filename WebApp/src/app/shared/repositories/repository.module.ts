import { NgModule } from '@angular/core';
import { CatalogueRepo, AccoutingRepo, OperationRepo, SystemRepo, DocumentationRepo, ReportRepo } from '.';

@NgModule({
    providers: [
        SystemRepo,
        AccoutingRepo,
        OperationRepo,
        CatalogueRepo,
        DocumentationRepo,
        ReportRepo
    ],
})
export class RepositoryModule {
}
