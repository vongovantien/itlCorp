import { Component } from '@angular/core';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'form-search-tariff',
    templateUrl: './form-search-tariff.component.html',
})
export class TariffFormSearchComponent extends AppForm {
    constructor() {
        super();
    }

    ngOnInit(): void { }
}
