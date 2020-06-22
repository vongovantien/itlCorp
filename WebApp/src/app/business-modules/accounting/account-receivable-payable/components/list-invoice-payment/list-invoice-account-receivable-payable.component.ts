import { Component, OnInit, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { AccountReceivablePayableUpdateExtendDayPopupComponent } from '../popup/update-extend-day/update-extend-day.popup';

@Component({
    selector: 'list-invoice-account-receivable-payable',
    templateUrl: './list-invoice-account-receivable-payable.component.html',
})
export class AccountReceivablePayableListInvoicePaymentComponent extends AppList implements OnInit {

    @ViewChild(AccountReceivablePayableUpdateExtendDayPopupComponent, { static: true }) updateExtendDayPopup: AccountReceivablePayableUpdateExtendDayPopupComponent;

    constructor() {
        super();
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Reference No', field: 'referenceNo', sortable: true },
            { title: 'Partner Name', field: 'referenceNo', sortable: true },
            { title: 'Invoice Amount', field: 'referenceNo', sortable: true },
            { title: 'Currency', field: 'referenceNo', sortable: true },
            { title: 'Invoice Date', field: 'referenceNo', sortable: true },
            { title: 'Serie No', field: 'referenceNo', sortable: true },
            { title: 'Paid Amount', field: 'referenceNo', sortable: true },
            { title: 'Unpaid Amount', field: 'referenceNo', sortable: true },
            { title: 'Due Date', field: 'referenceNo', sortable: true },
            { title: 'Overdue Days', field: 'referenceNo', sortable: true },
            { title: 'Payment Status', field: 'referenceNo', sortable: true },
            { title: 'Extends date', field: 'referenceNo', sortable: true },
            { title: 'Notes', field: 'referenceNo', sortable: true },
        ];
    }

    ngAfterViewInit() {
        this.updateExtendDayPopup.show();

    }
}

