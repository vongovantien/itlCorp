import { Component } from '@angular/core';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'buying-charge',
    templateUrl: './buying-charge.component.html',
})
export class ShareBussinessBuyingChargeComponent extends AppList {

    headers: CommonInterface.IHeaderTable[] = [];
    charges: any[] = [1, 2];

    constructor() {
        super();
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Charge', field: '', required: true },
            { title: 'Partner', field: '', required: true },
            { title: 'Q`ty', field: '', required: true },
            { title: 'Unit', field: '', required: true },
            { title: 'Unit Price', field: '', required: true },
            { title: 'Currency', field: '', required: true },
            { title: 'VAT', field: '', required: true },
            { title: 'Total Amount', field: '' },
            { title: 'Invoice No', field: '' },
            { title: 'Series No', field: '' },
            { title: 'Invoice Date', field: '' },
            { title: 'KB', field: '' },
            { title: 'Note', field: '' },
        ];
    }
}
