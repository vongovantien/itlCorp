import { NgModule } from '@angular/core';
import { CatalogueRepo, AccoutingRepo, UnitRepo, OperationRepo, SystemRepo, DocumentationRepo, ReportRepo } from '.';

@NgModule({
    providers: [
        SystemRepo,
        AccoutingRepo,
        UnitRepo,
        OperationRepo,
        CatalogueRepo,
        DocumentationRepo,
        ReportRepo
    ],
})
export class RepositoryModule {
}
