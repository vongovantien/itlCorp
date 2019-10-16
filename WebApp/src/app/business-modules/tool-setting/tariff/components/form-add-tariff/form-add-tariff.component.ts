import { Component } from '@angular/core';
import { TariffFormSearchComponent } from '../form-search-tariff/form-search-tariff.component';
import { DataService } from 'src/app/shared/services';
import { CatalogueRepo, SystemRepo } from 'src/app/shared/repositories';

@Component({
    selector: 'form-add-tariff',
    templateUrl: './form-add-tariff.component.html',
})
export class TariffFormAddComponent extends TariffFormSearchComponent {

    constructor(
        protected _dataService: DataService,
        protected _catalogueRepo: CatalogueRepo,
        protected _systemRepo: SystemRepo) {
        super(_dataService, _catalogueRepo, _systemRepo);
    }

}
