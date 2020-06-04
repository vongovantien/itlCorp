import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Router } from '@angular/router';

@Component({
    selector: 'app-accounting-voucher',
    templateUrl: './accounting-voucher.component.html'
})

export class AccountingManagementVoucherComponent extends AppList implements OnInit {
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
            case 'cdi': {
                this._router.navigate([`home/accounting/management/cd-invoice`]);
                break;
            }
        }
    }
}