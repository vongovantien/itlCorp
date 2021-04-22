import { OnInit, Component, ChangeDetectionStrategy } from "@angular/core";
import { AppList } from "@app";
import { Observable } from "rxjs";
import { Store } from "@ngrx/store";
import { ReceiptCreditListState } from "../../store/reducers";
import { IReceiptState } from "../../store/reducers/customer-payment.reducer";

@Component({
    selector: 'customer-payment-receipt-credit-list',
    templateUrl: './receipt-credit-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ARCustomerPaymentReceiptCreditListComponent extends AppList implements OnInit {

    creditList$: Observable<any>;

    constructor(
        private _store: Store<IReceiptState>
    ) {
        super();
    }

    ngOnInit() {
        this.headers = [
            { title: 'RefNo', field: '' },
            { title: 'Net Off Invoice No', field: '', width: 250 },
            { title: 'Org Amount', field: '' },
            { title: 'Amount USD', field: '' },
            { title: 'Amount VND', field: '' },
            { title: 'Note', field: '' },
            { title: 'BU Handle', field: '' },
            { title: 'Office', field: '' },
        ];

        this.creditList$ = this._store.select(ReceiptCreditListState);

    }
}
