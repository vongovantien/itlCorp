import { OnInit, Component, ChangeDetectionStrategy, EventEmitter, Output, Input } from "@angular/core";
import { AppList } from "@app";
import { Observable } from "rxjs";
import { IReceiptState } from "../../store/reducers/customer-payment.reducer";
import { Store } from "@ngrx/store";
import { ReceiptDebitListState, ReceiptTypeState } from "../../store/reducers";
import { ReceiptInvoiceModel } from "@models";
import { RemoveInvoice } from "../../store/actions";
import { takeUntil } from "rxjs/operators";

@Component({
    selector: 'customer-payment-receipt-debit-list',
    templateUrl: './receipt-debit-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ARCustomerPaymentReceiptDebitListComponent extends AppList implements OnInit {
    @Output() onChangeDebit: EventEmitter<boolean> = new EventEmitter<boolean>();
    @Input() isReadonly: boolean = false;
    
    debitList$: Observable<ReceiptInvoiceModel[]>;
    agencyHeaders: CommonInterface.IHeaderTable[];
    selectedIndexItem: number;
    receiptType: string = null;

    constructor(
        private _store: Store<IReceiptState>
    ) {
        super();
    }

    ngOnInit() {
        this.headers = [
            { title: 'RefNo', field: '', sortable: true },
            { title: 'Type', field: '' },
            { title: 'Invoice No', field: '', width: 150 },
            { title: 'Org Amount', field: '' },
            { title: 'Unpaid USD', field: '' },
            { title: 'Unpaid VND', field: '' },
            { title: 'Paid Amount USD', field: '' },
            { title: 'Paid Amount VND', field: '' },
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
            { title: 'Org Amount', field: '' },
            { title: 'Unpaid USD', field: '' },
            { title: 'Unpaid VND', field: '' },
            { title: 'Paid Amount USD', field: '' },
            { title: 'Paid Amount VND', field: '' },
            { title: 'Remain USD', field: '' },
            { title: 'Remain VND', field: '' },
            { title: 'Payment Note', field: '' },
            { title: 'BU Handle', field: '' },
            { title: 'Office', field: '' }
        ];
        this.debitList$ = this._store.select(ReceiptDebitListState);
        this.checkAllChange();
        this._store.select(ReceiptTypeState)
            .pipe()
            .subscribe(x => this.receiptType = x);
    }

    checkAllChange() {
        this.debitList$.pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((x: ReceiptInvoiceModel[]) => {
                x.forEach((element: ReceiptInvoiceModel) => {
                    element.isSelected = this.isCheckAll;
                });
            });
    }

    onCheckChange() {
        this.debitList$.pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((x: ReceiptInvoiceModel[]) => {
                this.isCheckAll = x.filter((element: ReceiptInvoiceModel) => !element.isSelected).length === 0;
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

    formatAmount(event: any, receipt: ReceiptInvoiceModel){
        if(!event.target.value.length){
            receipt[event.target.name] = 0;
        }
    }
}
