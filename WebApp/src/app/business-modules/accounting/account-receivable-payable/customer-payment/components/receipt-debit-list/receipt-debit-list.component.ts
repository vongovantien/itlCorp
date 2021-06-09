import { OnInit, Component, ChangeDetectionStrategy, EventEmitter, Output, Input } from "@angular/core";
import { AppList } from "@app";
import { Observable } from "rxjs";
import { IReceiptState } from "../../store/reducers/customer-payment.reducer";
import { Store } from "@ngrx/store";
import { ReceiptDebitListState, ReceiptTypeState, ReceiptCreditListState } from "../../store/reducers";
import { ReceiptInvoiceModel } from "@models";
import { RemoveInvoice, ClearCredit } from "../../store/actions";
import { takeUntil } from "rxjs/operators";
import { DataService } from "@services";

@Component({
    selector: 'customer-payment-receipt-debit-list',
    templateUrl: './receipt-debit-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ARCustomerPaymentReceiptDebitListComponent extends AppList implements OnInit {
    @Output() onChangeDebit: EventEmitter<boolean> = new EventEmitter<boolean>();
    @Input() isReadonly: boolean = false;

    debitList$: Observable<ReceiptInvoiceModel[]>;
    creditList$: Observable<ReceiptInvoiceModel[]>;

    agencyHeaders: CommonInterface.IHeaderTable[];
    selectedIndexItem: number;
    receiptType: string = null;

    selectedCredit: ReceiptInvoiceModel;

    isSubmitted: boolean = false;

    constructor(
        private _store: Store<IReceiptState>,
        private _dataService: DataService
    ) {
        super();
    }

    ngOnInit() {
        this.headers = [
            { title: 'RefNo', field: '', sortable: true },
            { title: 'Type', field: '' },
            { title: 'Invoice No', field: '', width: 150 },
            { title: 'Credit No', field: '', width: 250 },
            { title: 'Org Amount', field: '', align: this.right },
            { title: 'Unpaid USD', field: '' },
            { title: 'Unpaid VND', field: '' },
            { title: 'Paid Amount USD', field: '', align: this.right },
            { title: 'Paid Amount VND', field: '', align: this.right },
            { title: 'Total Paid VND', field: '', align: this.right },
            { title: 'Total Paid USD', field: '', align: this.right },
            { title: 'Remain USD', field: '' },
            { title: 'Remain VND', field: '' },
            { title: 'Payment Note', field: '' },
            { title: 'BU Handle', field: '' },
            { title: 'Office', field: '' },
        ];
        this.agencyHeaders = [
            { title: 'RefNo', field: '', sortable: true },
            { title: 'Type', field: '' },
            { title: 'Invoice No', field: '', width: 150 },
            { title: 'Job No', field: '' },
            { title: 'MBL No', field: '' },
            { title: 'HBL No', field: '' },
            { title: 'Org Amount', field: '', align: this.right },
            { title: 'Unpaid USD', field: '' },
            { title: 'Unpaid VND', field: '' },
            { title: 'Paid Amount USD', field: '', align: this.right },
            { title: 'Paid Amount VND', field: '', align: this.right },
            { title: 'Remain USD', field: '' },
            { title: 'Remain VND', field: '' },
            { title: 'Payment Note', field: '' },
            { title: 'BU Handle', field: '' },
            { title: 'Office', field: '' }
        ];
        this.debitList$ = this._store.select(ReceiptDebitListState);
        this.creditList$ = this._store
            .select(ReceiptCreditListState)

        this._store.select(ReceiptTypeState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(x => this.receiptType = x || 'Customer');
    }

    checkAllChange() {
        this.debitList$.pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((x: ReceiptInvoiceModel[]) => {
                x.forEach((element: ReceiptInvoiceModel) => {
                    element.isSelected = this.isCheckAll;
                });
            });
    }

    confirmDeleteInvoiceItem(index: number) {
        this.selectedIndexItem = index;
    }

    onDeleteInvoiceItem() {
        if (this.selectedIndexItem === undefined) {
            return;
        }
        this._store.dispatch(RemoveInvoice({ index: this.selectedIndexItem }));
        this.onChangeDebit.emit(true);
    }

    formatAmount(event: any, receipt: ReceiptInvoiceModel) {
        if (!event.target.value.length) {
            receipt[event.target.name] = 0;
        }
    }

    onChangeCalCredit(_refNo: string, curr: ReceiptInvoiceModel) {
        console.log(_refNo);
        if (!!_refNo) {
            this.creditList$
                .pipe(takeUntil(this.ngUnsubscribe))
                .subscribe(
                    (credits: ReceiptInvoiceModel[]) => {
                        if (!!credits.length) {
                            const currentCredit = credits.find(x => x.refNo == _refNo);
                            curr.creditNo = _refNo;
                            curr.totalPaidVnd = currentCredit?.unpaidAmountVnd + curr.paidAmountVnd;
                            curr.totalPaidUsd = currentCredit?.unpaidAmountUsd + curr.paidAmountUsd;

                            this._dataService.setData('clearCredit', { invoiceNo: curr.invoiceNo, creditNo: _refNo });
                        }
                    }
                )
        }
    }
}
