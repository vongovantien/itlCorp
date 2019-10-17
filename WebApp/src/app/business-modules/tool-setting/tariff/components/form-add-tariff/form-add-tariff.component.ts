import { Component } from '@angular/core';
import { TariffFormSearchComponent } from '../form-search-tariff/form-search-tariff.component';
import { DataService } from 'src/app/shared/services';
import { CatalogueRepo, SystemRepo, SettingRepo } from 'src/app/shared/repositories';
import { FormBuilder } from '@angular/forms';

@Component({
    selector: 'form-add-tariff',
    templateUrl: './form-add-tariff.component.html',
})
export class TariffFormAddComponent extends TariffFormSearchComponent {

    constructor(
        protected _dataService: DataService,
        protected _catalogueRepo: CatalogueRepo,
        protected _fb: FormBuilder,
        protected _systemRepo: SystemRepo) {
        super(_dataService, _catalogueRepo, _systemRepo, _fb);
    }

}
