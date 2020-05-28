import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Router } from '@angular/router';

type TAB = 'CDI' | 'VAT' | 'VOUCHER';
@Component({
    selector: 'app-accounting-debit-credit-invoice',
    templateUrl: './accounting-debit-credit-invoice.component.html'
})

export class AccountingManagementDebitCreditInvoiceComponent extends AppList implements OnInit {

    selectedTab: TAB = 'CDI';

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
            case 'vat': {
                this._router.navigate([`home/accounting/management/vat-invoice`]);
                break;
            }
            case 'voucher': {
                this._router.navigate([`home/accounting/management/voucher`]);
                break;
            }
        }
    }
}
