import { OnInit, Component, Input, ViewContainerRef, ViewChildren, QueryList, Output, EventEmitter, ViewChild } from "@angular/core";
import { AppList } from "@app";
import { Observable } from "rxjs";
import { Store } from "@ngrx/store";
import { ReceiptCreditListState, ReceiptDebitListState, ReceiptTypeState } from "../../store/reducers";
import { IReceiptState } from "../../store/reducers/customer-payment.reducer";
import { ReceiptInvoiceModel } from "@models";
import { distinctUntilChanged, skip, takeUntil, filter, pluck } from "rxjs/operators";
import { RemoveCredit } from "../../store/actions";
import { AppComboGridComponent, ConfirmPopupComponent } from "@common";
import { DataService } from "@services";
import _cloneDeep from 'lodash/cloneDeep'
import { InjectViewContainerRefDirective } from "@directives";
@Component({
    selector: 'customer-payment-receipt-credit-list',
    templateUrl: './receipt-credit-list.component.html',
    // changeDetection: ChangeDetectionStrategy.OnPush
})
export class ARCustomerPaymentReceiptCreditListComponent extends AppList implements OnInit {
    @ViewChildren('container', { read: ViewContainerRef }) public widgetTargets: QueryList<ViewContainerRef>;
    @ViewChild(InjectViewContainerRefDirective) injectViewContainer: InjectViewContainerRefDirective;
    @Output() onChangeCredit: EventEmitter<boolean> = new EventEmitter<boolean>();
    @Input() isReadonly: boolean = false;

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

    private _type: string = 'Customer'

    creditList: ReceiptInvoiceModel[] = [];
    debitList: Observable<ReceiptInvoiceModel[]> = this._store.select(ReceiptDebitListState);

    agencyHeaders: CommonInterface.IHeaderTable[] = [
        { title: 'RefNo', field: '' },
        { title: 'Net Off Invoice No', field: '' },
        { title: 'Job', field: '', width: 150 },
        { title: 'HBL', field: '', width: 150 },
        { title: 'MBL', field: '', width: 150 },
        { title: 'Org Amount', field: '', align: this.right, width: 150 },
        { title: 'Amount USD', field: '', width: 150, align: this.right },
        { title: 'Amount VND', field: '', width: 150, align: this.right },
        { title: 'Note', field: '', width: 200 },
        { title: 'BU Handle', field: '' },
        { title: 'Office', field: '' },
    ];
    configDebitDisplayFields: CommonInterface.IComboGridDisplayField[] = [
        { field: 'invoiceNo', label: 'Invoice No' },
        { field: 'amount', label: 'Unpaid Invoice' }
    ];
    isSubmitted: boolean = false;
    selectedInvoice: ReceiptInvoiceModel;
    selectedIndexInvoice: number = -1;
    headerInvoice: CommonInterface.IHeaderTable[] = [
        { field: 'invoiceNo', title: 'Invoice No' },
        { field: 'amount', title: 'Unpaid Invoice' }
    ];
    invoiceDatasource: any[] = [];
    receiptType: string = null;

    constructor(
        private readonly _store: Store<IReceiptState>,
        private readonly dataService: DataService
    ) {
        super();
    }

    ngOnInit() {
        this.headers = [
            { title: 'RefNo', field: '' },
            { title: 'Net Off Invoice No', field: '' },
            { title: 'Org Amount', field: '', align: this.right, width: 150 },
            { title: 'Amount USD', field: '', width: 150, align: this.right },
            { title: 'Amount VND', field: '', width: 150, align: this.right },
            { title: 'Note', field: '', width: 200 },
            { title: 'BU Handle', field: '' },
            { title: 'Office', field: '' },
        ];

        this._store.select(ReceiptCreditListState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (data) => {
                    this.creditList = data;
                    console.log(this.creditList);
                }
            )

        this._store.select(ReceiptTypeState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(x => this.receiptType = x || 'Customer');


        // * Listen Debit clear Credit
        this.dataService.currentMessage
            .pipe(
                filter(x => !!x['clearCredit']),
                pluck('clearCredit'),
                takeUntil(this.ngUnsubscribe))
            .subscribe(
                (data: { invoiceNo: string, creditNo: string }) => {
                    if (data.creditNo) {

                        const indexCreditCurrent = this.creditList.findIndex(x => x.refNo === data.creditNo)
                        if (indexCreditCurrent !== -1) {
                            this.creditList[indexCreditCurrent].invoiceNo = data.invoiceNo;
                        }
                    }
                }
            )
    }

    formatNumberCurrency(input: number) {
        return input.toLocaleString(
            'en-US', // leave undefined to use the browser's locale, or use a string like 'en-US' to override it.
            { minimumFractionDigits: 2 }
        );
    }

    onSelectInvoice(data: any, invoiceItem: ReceiptInvoiceModel) {
        if (!!data) {
            invoiceItem.invoiceNo = data.invoiceNo;
        }
    }

    confirmDeleteInvoiceItem(item: ReceiptInvoiceModel, index: number) {
        this.selectedIndexInvoice = index;
        if (item.type === "OBH") {
            this.showPopupDynamicRender(ConfirmPopupComponent, this.injectViewContainer.viewContainerRef, {
                title: 'Confirm Remove Credit',
                body: 'Some Credit Values will change, do you want to remove you selections?',
                labelConfirm: 'Yes',
                labelCancel: 'No'
            }, () => {
                this.onDeleteInvoiceItem();
            })
        } else if (!!item.id) {
            this.showPopupDynamicRender(ConfirmPopupComponent, this.injectViewContainer.viewContainerRef, {
                title: 'Confirm Remove Credit',
                body: 'Some Credit Values will change, do you want to remove you selections?',
                labelConfirm: 'Yes',
                labelCancel: 'No'
            }, () => {
                this.onDeleteInvoiceItem();
            })
        } else {
            this.onDeleteInvoiceItem();
        }
    }

    onDeleteInvoiceItem() {
        if (this.selectedIndexInvoice === -1) {
            return;
        }
        this._store.dispatch(RemoveCredit({ index: this.selectedIndexInvoice }));
        this.onChangeCredit.emit(true);
    }
}
