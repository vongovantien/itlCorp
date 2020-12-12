import { OnInit, Component } from "@angular/core";
import { AppList } from "@app";
import { AppPage } from "src/app/app.base";
import { DataService } from "@services";
import { pluck, takeUntil } from "rxjs/operators";
import { ReceiptInvoiceModel } from "@models";

@Component({
    selector: 'customer-payment-receipt-summary',
    templateUrl: './receipt-summary.component.html',
})
export class ARCustomerPaymentReceiptSummaryComponent extends AppList implements OnInit {

    invoices: ReceiptInvoiceModel[] = [];

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

        this._dataService.currentMessage
            .pipe(pluck('invoices-list-change'), takeUntil(this.ngUnsubscribe))
            .subscribe(
                (invoices: ReceiptInvoiceModel[]) => {
                    if (invoices !== undefined) {
                        this.invoices = [...invoices];
                    }
                }
            )
    }

    calculateInfodataInvoice() {

    }
}

