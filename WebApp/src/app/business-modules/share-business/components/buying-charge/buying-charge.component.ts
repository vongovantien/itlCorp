import { Component } from '@angular/core';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'buying-charge',
    templateUrl: './buying-charge.component.html',
})
export class ShareBussinessBuyingChargeComponent extends AppList {

    headers: CommonInterface.IHeaderTable[] = [];

    constructor() {
        super();
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Charge', field: '' },
            { title: 'Partner', field: '' },
            { title: 'Q`ty', field: '' },
            { title: 'Unit', field: '' },
            { title: 'Unit Price', field: '' },
            { title: 'Currency', field: '' },
            { title: 'VAT', field: '' },
            { title: 'Total Amount', field: '' },
            { title: 'Invoice No', field: '' },
            { title: 'Series No', field: '' },
            { title: 'Invoice Date', field: '' },
            { title: 'KB', field: '' },
            { title: 'Note', field: '' },
        ];
    }
}
