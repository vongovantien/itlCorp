import { Component } from '@angular/core';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'app-add-tariff',
    templateUrl: './add-tariff.component.html',
})
export class TariffAddComponent extends AppList {
    constructor() {
        super();
    }

    ngOnInit(): void { }
}
