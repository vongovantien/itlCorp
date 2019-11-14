import { NgModule } from '@angular/core';
import { CatalogueRepo, AccountingRepo, OperationRepo, SystemRepo, DocumentationRepo, ExportRepo, SettingRepo } from '.';

@NgModule({
    providers: [
        SystemRepo,
        AccountingRepo,
        OperationRepo,
        CatalogueRepo,
        DocumentationRepo,
        ExportRepo,
        SettingRepo,
    ],
})
export class RepositoryModule {
}
