import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Router } from '@angular/router';

@Component({
    selector: 'app-accounting-vat-invoice',
    templateUrl: './accounting-vat-invoice.component.html'
})

export class AccountingManagementVatInvoiceComponent extends AppList implements OnInit {

    constructor(
        private _router: Router
    ) {
        super();
    }

    ngOnInit() {
        this.headers = [
            { title: 'Invoice No', field: '', sortable: true },
            { title: 'Job ID', field: '', sortable: true },
            { title: 'HBL', field: '', sortable: true },
            { title: 'Partner Name', field: '', sortable: true },
            { title: 'Total Amount', field: '', sortable: true },
            { title: 'Currency', field: '', sortable: true },
            { title: 'Issue Date', field: '', sortable: true },
            { title: 'Creator', field: '', sortable: true },
            { title: 'Status', field: '', sortable: true },
        ];
    }

    onSelectTab(tabName: string) {
        switch (tabName) {
            case 'cdi': {
                this._router.navigate([`home/accounting/management/cd-invoice`]);
                break;
            }
            case 'voucher': {
                this._router.navigate([`home/accounting/management/voucher`]);
                break;
            }
        }
    }
}
