import { OnInit, Component, ChangeDetectionStrategy, Input } from "@angular/core";
import { AppList } from "@app";
import { Observable } from "rxjs";
import { Store } from "@ngrx/store";
import { ReceiptCreditListState } from "../../store/reducers";
import { IReceiptState } from "../../store/reducers/customer-payment.reducer";
import { ReceiptCreditDebitModel } from "@models";
import { map, reduce, takeUntil } from "rxjs/operators";

@Component({
    selector: 'customer-payment-receipt-credit-list',
    templateUrl: './receipt-credit-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ARCustomerPaymentReceiptCreditListComponent extends AppList implements OnInit {

    @Input() set type(t: string) {
        if (!!t) {
            this._type = t;
            if (this._type !== 'Customer') {
                this.headers = Array.from([
                    { title: 'RefNo', field: 'refNo', sortable: true },
                    { title: 'JobNo', field: '' },
                    { title: 'MBL', field: '' },
                    { title: 'HBL', field: '' },
                    { title: 'Net Off Invoice No', field: '', width: 250 },
                    { title: 'Org Amount', field: '' },
                    { title: 'Amount USD', field: '' },
                    { title: 'Amount VND', field: '' },
                    { title: 'Note', field: '' },
                    { title: 'BU Handle', field: '' },
                    { title: 'Office', field: '' },
                ]);
            }
        }
    }

    totalOrgAmount: number;

    get type() {
        return this._type;
    }

    private _type: string = 'Customer' // Agent
    creditList: Observable<ReceiptCreditDebitModel[]>;

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

        this.creditList = this._store.select(ReceiptCreditListState);


    }
}
