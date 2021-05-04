import { OnInit, Component, ChangeDetectionStrategy, Input, ViewChild, Output, EventEmitter } from "@angular/core";
import { AppList } from "@app";
import { Observable } from "rxjs";
import { Store } from "@ngrx/store";
import { ReceiptCreditListState, ReceiptDebitListState } from "../../store/reducers";
import { IReceiptState } from "../../store/reducers/customer-payment.reducer";
import { ReceiptInvoiceModel } from "@models";
import { takeUntil } from "rxjs/operators";
import { RemoveCredit } from "../../store/actions";

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
                    { title: 'Net Off Invoice No', field: '', width: 250 },
                    { title: 'Org Amount', field: '' },
                    { title: 'Amount USD', field: '' },
                    { title: 'Amount VND', field: '' },
                    { title: 'Payment Note', field: '' },
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
    creditList: Observable<ReceiptInvoiceModel[]>;
    debitList$: Observable<ReceiptInvoiceModel[]>;
    configDebitDisplayFields: CommonInterface.IComboGridDisplayField[];
    isSubmitted: boolean = false;

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

        this.configDebitDisplayFields = [
            { field: 'invoiceNo', label: 'Invoice No' },
            { field: 'amount', label: 'Unpaid Invoice' }
        ];
        this.isCheckAll = false;
        this.creditList = this._store.select(ReceiptCreditListState);
        this.debitList$ = this._store.select(ReceiptDebitListState);

    }

    checkAllChange() {
        this.creditList.pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((x: ReceiptInvoiceModel[]) => {
                x.every((element: ReceiptInvoiceModel) => {
                    element.isSelected = this.isCheckAll;
                });
            });
    }

    onCheckChange() {
        this.creditList.pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((x: ReceiptInvoiceModel[]) => {
                this.isCheckAll = x.filter((element: ReceiptInvoiceModel) => !element.isSelected).length === 0;
            });
    }

    removeListItem(){
        this.creditList.pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((x: ReceiptInvoiceModel[]) => {
                if (x.filter((item: ReceiptInvoiceModel) => item.isSelected).length > 0) {
                    for (let i = 0; i < x.length; i++) {
                        if (x[i].isSelected === true) {
                            this._store.dispatch(RemoveCredit({ index: i }));
                        }
                    }
                }
            }).unsubscribe();
    }
}
