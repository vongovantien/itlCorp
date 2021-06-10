import { OnInit, Component, ChangeDetectionStrategy, Input, ViewContainerRef, ViewChildren, QueryList, Output, EventEmitter } from "@angular/core";
import { AppList } from "@app";
import { Observable } from "rxjs";
import { Store } from "@ngrx/store";
import { ReceiptCreditListState, ReceiptDebitListState, ReceiptTypeState } from "../../store/reducers";
import { IReceiptState } from "../../store/reducers/customer-payment.reducer";
import { ReceiptInvoiceModel } from "@models";
import { distinctUntilChanged, skip, takeUntil, filter, pluck } from "rxjs/operators";
import { RemoveCredit } from "../../store/actions";
import { AppComboGridComponent } from "@common";
import { DataService } from "@services";
import _cloneDeep from 'lodash/cloneDeep'
@Component({
    selector: 'customer-payment-receipt-credit-list',
    templateUrl: './receipt-credit-list.component.html',
    // changeDetection: ChangeDetectionStrategy.OnPush
})
export class ARCustomerPaymentReceiptCreditListComponent extends AppList implements OnInit {
    @ViewChildren('container', { read: ViewContainerRef }) public widgetTargets: QueryList<ViewContainerRef>;
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

    private _type: string = 'Customer' // Agent

    // creditList: Observable<ReceiptInvoiceModel[]>;
    creditList: ReceiptInvoiceModel[] = [];
    debitList: Observable<ReceiptInvoiceModel[]>;

    agencyHeaders: CommonInterface.IHeaderTable[];
    configDebitDisplayFields: CommonInterface.IComboGridDisplayField[];
    isSubmitted: boolean = false;
    selectedInvoice: ReceiptInvoiceModel;
    selectedIndexInvoice: number = -1;
    headerInvoice: CommonInterface.IHeaderTable[] = [];
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
            { title: 'Org Amount', field: '', align: this.right },
            { title: 'Amount USD', field: '', align: this.right },
            { title: 'Amount VND', field: '', align: this.right },
            { title: 'Note', field: '' },
            { title: 'BU Handle', field: '' },
            { title: 'Office', field: '' },
        ];
        this.agencyHeaders = [
            { title: 'RefNo', field: '' },
            { title: 'Job', field: '' },
            { title: 'HBL', field: '' },
            { title: 'MBL', field: '' },
            { title: 'Net Off Invoice No', field: '' },
            { title: 'Org Amount', field: '', align: this.right },
            { title: 'Amount USD', field: '', align: this.right },
            { title: 'Amount VND', field: '', align: this.right },
            { title: 'Payment Note', field: '' },
            { title: 'BU Handle', field: '' },
            { title: 'Office', field: '' },
        ];

        this.configDebitDisplayFields = [
            { field: 'invoiceNo', label: 'Invoice No' },
            { field: 'amount', label: 'Unpaid Invoice' }
        ];
        this.debitList = this._store.select(ReceiptDebitListState);
        this._store.select(ReceiptCreditListState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (data) => {
                    console.log(data);
                    this.creditList = data;
                }
            )

        this.getInvoiceList();

        this.headerInvoice = [
            { field: 'invoiceNo', title: 'Invoice No' },
            { field: 'amount', title: 'Unpaid Invoice' }
        ];
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
                        this.creditList.forEach(x => {
                            x.invoiceNo = null;
                        })
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


    getInvoiceList() {
        this.debitList.pipe(distinctUntilChanged(), takeUntil(this.ngUnsubscribe))
            .subscribe((x: ReceiptInvoiceModel[]) => {
                this.invoiceDatasource = [];
                x.filter((element: ReceiptInvoiceModel) => (element.type !== 'ADV' && element.type !== 'OBH')).map((element: ReceiptInvoiceModel) => {
                    const item = {
                        invoiceNo: element.invoiceNo,
                        amount: this.formatNumberCurrency(element.amount),
                    };
                    this.invoiceDatasource = [...this.invoiceDatasource, item];
                })
            });
    }


    onSelectInvoice(data: any, invoiceItem: ReceiptInvoiceModel) {
        if (!!data) {
            invoiceItem.invoiceNo = data.invoiceNo;
        }
    }

    loadDynamicComoGrid(item: ReceiptInvoiceModel, index: number) {
        this.selectedInvoice = item;
        this.selectedIndexInvoice = index;
        const containerRef: ViewContainerRef = this.widgetTargets.toArray()[index];
        this.componentRef = this.renderDynamicComponent(AppComboGridComponent, containerRef);
        if (!!this.componentRef) {
            this.getInvoiceList();
            this.componentRef.instance.headers = this.headerInvoice;
            this.componentRef.instance.data = this.invoiceDatasource;
            this.componentRef.instance.active = item.invoiceNo;

            this.subscription = ((this.componentRef.instance) as AppComboGridComponent<ReceiptInvoiceModel>).onClick.subscribe(
                (v: ReceiptInvoiceModel) => {
                    this.onSelectInvoice(v, this.selectedInvoice);
                    this.subscription.unsubscribe();

                    containerRef.clear();
                });
            ((this.componentRef.instance) as AppComboGridComponent<ReceiptInvoiceModel>).clickOutSide
                .pipe(skip(1))
                .subscribe(
                    () => {
                        containerRef.clear();
                    }
                );
        }
    }

    confirmDeleteInvoiceItem(index: number) {
        this.selectedIndexInvoice = index;
    }

    onDeleteInvoiceItem() {
        if (this.selectedIndexInvoice === -1) {
            return;
        }
        this._store.dispatch(RemoveCredit({ index: this.selectedIndexInvoice }));
        this.onChangeCredit.emit(true);
    }
}
