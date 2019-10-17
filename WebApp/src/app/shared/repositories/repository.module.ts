import { NgModule } from '@angular/core';
import { CatalogueRepo, AccountingRepo, OperationRepo, SystemRepo, DocumentationRepo, ExportRepo } from '.';
import { SettingRepo } from './setting.repo';

@NgModule({
    providers: [
        SystemRepo,
        AccountingRepo,
        OperationRepo,
        CatalogueRepo,
        DocumentationRepo,
        ExportRepo,
        SettingRepo
    ],
})
export class RepositoryModule {
}
