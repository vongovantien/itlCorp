import { OnInit, Component, ChangeDetectionStrategy } from "@angular/core";
import { AppList } from "@app";
import { DataService } from "@services";
import { pluck, takeUntil } from "rxjs/operators";
import { ReceiptInvoiceModel } from "@models";

@Component({
    selector: 'customer-payment-receipt-summary',
    templateUrl: './receipt-summary.component.html',
})
export class ARCustomerPaymentReceiptSummaryComponent extends AppList implements OnInit {

    invoices: ReceiptInvoiceModel[] = [];
    totalDebit: number = 0;
    totalOBH: number;
    totalAdv: number;

    constructor(
        private _dataService: DataService
    ) {
        super();
    }

    ngOnInit() {
        this.headers = [
            { field: '', title: 'Total Summary' },
            { field: '', title: 'Total Debit' },
            { field: '', title: 'Total OBH' },
            { field: '', title: 'Total ADV' },
        ];
    }

    calculateInfodataInvoice(invoice: ReceiptInvoiceModel[]) {
        if (invoice.length) {
            this.totalDebit = invoice.filter(x => x.type === 'DEBIT').reduce((acc: any, curr: ReceiptInvoiceModel) => acc += curr.paidAmount, 0);
            this.totalOBH = invoice.filter(x => x.type === 'OBH').reduce((acc: any, curr: ReceiptInvoiceModel) => acc += curr.paidAmount, 0);
            this.totalAdv = invoice.filter(x => x.type === 'ADV').reduce((acc: any, curr: ReceiptInvoiceModel) => acc += curr.paidAmount, 0);
        }
    }
}

