import { OnInit, Component, ChangeDetectionStrategy, Input, ViewContainerRef, ViewChildren, QueryList } from "@angular/core";
import { AppList } from "@app";
import { Observable } from "rxjs";
import { Store } from "@ngrx/store";
import { ReceiptCreditListState, ReceiptDebitListState } from "../../store/reducers";
import { IReceiptState } from "../../store/reducers/customer-payment.reducer";
import { ReceiptInvoiceModel } from "@models";
import { skip, takeUntil } from "rxjs/operators";
import { RemoveCredit } from "../../store/actions";
import { AppComboGridComponent } from "@common";

@Component({
    selector: 'customer-payment-receipt-credit-list',
    templateUrl: './receipt-credit-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ARCustomerPaymentReceiptCreditListComponent extends AppList implements OnInit {
    @ViewChildren('container', { read: ViewContainerRef }) public widgetTargets: QueryList<ViewContainerRef>;
    
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
    debitList: Observable<ReceiptInvoiceModel[]>;
    configDebitDisplayFields: CommonInterface.IComboGridDisplayField[];
    isSubmitted: boolean = false;
    selectedInvoice: ReceiptInvoiceModel;
    selectedIndexInvoice: number = -1;
    headerInvoice: CommonInterface.IHeaderTable[] = [];
    invoiceDatasource: any[] = [];

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
        this.creditList = this._store.select(ReceiptCreditListState);
        this.debitList = this._store.select(ReceiptDebitListState);
        this.checkAllChange();
        this.getInvoiceList();
        this.headerInvoice = [
            { field: 'invoiceNo', title: 'Invoice No' },
            { field: 'amount', title: 'Unpaid Invoice' }
        ];
    }

    formatNumberCurrency(input: number) {
        return input.toLocaleString(
            'en-US', // leave undefined to use the browser's locale, or use a string like 'en-US' to override it.
            { minimumFractionDigits: 2 }
        );
    }

    checkAllChange() {
        this.creditList.pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((x: ReceiptInvoiceModel[]) => {
                x.forEach((element: ReceiptInvoiceModel) => {
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

    removeListItem() {
        this.creditList.pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((x: ReceiptInvoiceModel[]) => {
                const removeList = x.filter((item: ReceiptInvoiceModel) => item.isSelected);
                if (removeList.length > 0) {
                    for (let i = 0; i < removeList.length; i++) {
                        this._store.dispatch(RemoveCredit({ index: i }));
                    }
                }
            }).unsubscribe();
    }

    getInvoiceList(){
        this.debitList.pipe(takeUntil(this.ngUnsubscribe))
        .subscribe((x: ReceiptInvoiceModel[]) => {
            x.forEach((element: ReceiptInvoiceModel) => {
                const item = {
                    invoiceNo: element.amount,
                    amount: this.formatNumberCurrency(element.amount),
                };
                this.invoiceDatasource = [...this.invoiceDatasource, item]
            });
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
            
            this.componentRef.instance.headers = this.headerInvoice;
            this.componentRef.instance.data = this.invoiceDatasource;
            // this.componentRef.instance.fields = ['invoiceNo', "amount | number: '.3-3'"];
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
}
