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

    @Input() isReadonly: boolean = false;

    creditList: ReceiptInvoiceModel[] = [];
    debitList: Observable<ReceiptInvoiceModel[]> = this._store.select(ReceiptDebitListState);

    agencyHeaders: CommonInterface.IHeaderTable[] = [
        { title: 'RefNo', field: '' },
        { title: 'Net Off Invoice No', field: '' },
        { title: 'Acct Doc', field: '', },
        { title: 'Amount USD', field: '', width: 150, align: this.right },
        { title: 'Amount VND', field: '', width: 150, align: this.right },
        { title: 'NetOff USD', field: '', width: 150, required: true, align: this.right },
        { title: 'NetOff VND', field: '', width: 150, required: true, align: this.right },
        { title: 'Balance USD', field: '', width: 150, align: this.right },
        { title: 'Balance VND', field: '', width: 150, align: this.right },
        { title: 'Job', field: '', width: 150 },
        { title: 'HBL', field: '', width: 150 },
        { title: 'MBL', field: '', width: 150 },
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
    receiptType$: Observable<string> = this._store.select(ReceiptTypeState);

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
            { title: 'Acct Doc', field: '', },
            { title: 'Amount USD', field: '', width: 150, align: this.right },
            { title: 'Amount VND', field: '', width: 150, align: this.right },
            { title: 'Amount USD', field: '', width: 150, align: this.right },
            { title: 'Amount VND', field: '', width: 150, align: this.right },
            { title: 'NetOff USD', field: '', width: 150, required: true, align: this.right },
            { title: 'NetOff VND', field: '', width: 150, required: true, align: this.right },
            { title: 'Balance USD', field: '', width: 150, align: this.right },
            { title: 'Balance VND', field: '', width: 150, align: this.right },
            { title: 'Note', field: '', width: 200 },
            { title: 'BU Handle', field: '' },
            { title: 'Office', field: '' },
        ];

        this._store.select(ReceiptCreditListState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (data) => {
                    this.creditList = data;
                }
            )

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
    }
}
