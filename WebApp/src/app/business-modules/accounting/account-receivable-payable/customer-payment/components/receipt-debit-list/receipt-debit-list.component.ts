import { OnInit, Component, ChangeDetectionStrategy, EventEmitter, Output, Input, ViewChild, ChangeDetectorRef } from "@angular/core";
import { AppList } from "@app";
import { Store } from "@ngrx/store";
import { DataService } from "@services";
import { ReceiptInvoiceModel } from "@models";
import { ConfirmPopupComponent } from "@common";
import { InjectViewContainerRefDirective } from "@directives";
import { ToastrService } from "ngx-toastr";

import { IReceiptState } from "../../store/reducers/customer-payment.reducer";
import { ReceiptDebitListState, ReceiptTypeState, ReceiptCreditListState } from "../../store/reducers";
import { RemoveInvoice } from "../../store/actions";

import { takeUntil } from "rxjs/operators";
import { Observable } from "rxjs";

@Component({
    selector: 'customer-payment-receipt-debit-list',
    templateUrl: './receipt-debit-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ARCustomerPaymentReceiptDebitListComponent extends AppList implements OnInit {
    @ViewChild(InjectViewContainerRefDirective) viewContainerInject: InjectViewContainerRefDirective;
    @Output() onChangeDebit: EventEmitter<boolean> = new EventEmitter<boolean>();
    @Input() isReadonly: boolean = false;

    debitList$ = this._store.select(ReceiptDebitListState);
    creditList$ = this._store.select(ReceiptCreditListState);

    agencyHeaders: CommonInterface.IHeaderTable[] = [
        { title: 'RefNo', field: '', sortable: true },
        { title: 'Type', field: '' },
        { title: 'Invoice No', field: '' },
        { title: 'Credit No', field: '', width: 250 },
        { title: 'Job No', field: '', width: 150 },
        { title: 'MBL No', field: '', width: 150 },
        { title: 'HBL No', field: '', width: 150 },
        { title: 'Unpaid USD', field: '', width: 150 },
        { title: 'Unpaid VND', field: '', width: 150 },
        { title: 'Paid Amount USD', field: '', width: 150, align: this.right },
        { title: 'Paid Amount VND', field: '', width: 150, align: this.right },
        { title: 'Total Paid VND', field: '', width: 150, align: this.right },
        { title: 'Total Paid USD', field: '', width: 150, align: this.right },
        { title: 'Remain USD', field: '', width: 150 },
        { title: 'Remain VND', field: '', width: 150 },
        { title: 'Note', field: '', width: 200 },
        { title: 'BU Handle', field: '' },
        { title: 'Office', field: '' }
    ];
    selectedIndexItem: number;
    receiptType$: Observable<string> = this._store.select(ReceiptTypeState);

    selectedCredit: ReceiptInvoiceModel;

    isSubmitted: boolean = false;
    sumTotalObj = {
        totalUnpaidAmountUsd: 0,
        totalUnpaidAmountVnd: 0,
        totalPaidAmountVnd: 0,
        totalPaidAmountUsd: 0,
        totalPaidVnd: 0,
        totalPaidUsd: 0,
        totalRemainVnd: 0,
        totalRemainUsd: 0
    };
    constructor(
        private readonly _store: Store<IReceiptState>,
        private readonly _dataService: DataService,
        private readonly _toastService: ToastrService,
    ) {
        super();
    }

    ngOnInit() {
        this.headers = [
            { title: 'RefNo', field: '', sortable: true },
            { title: 'Type', field: '' },
            { title: 'Invoice No', field: '' },
            { title: 'Credit No', field: '', width: 180 },
            { title: 'Unpaid USD', field: '', width: 150 },
            { title: 'Unpaid VND', field: '', width: 150 },
            { title: 'Paid Amount USD', field: '', width: 150, align: this.right },
            { title: 'Paid Amount VND', field: '', width: 150, align: this.right },
            { title: 'Total Paid USD', field: '', width: 150, align: this.right },
            { title: 'Total Paid VND', field: '', width: 150, align: this.right },
            { title: 'Remain USD', field: '', width: 150 },
            { title: 'Remain VND', field: '', width: 150 },
            { title: 'Note', field: '', width: 200 },
            { title: 'BU Handle', field: '' },
            { title: 'Office', field: '' },
        ];

        this.debitList$.pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (debitList: ReceiptInvoiceModel[]) => {
                    if (!!debitList.length) {
                        console.log(debitList);
                        this.sumTotalObj = this.calculateTotal(debitList);
                        // calculateTotal();
                    }
                }
            )
    }

    confirmDeleteInvoiceItem(item: ReceiptInvoiceModel, index: number) {
        this.selectedIndexItem = index;
        if (item.type === "OBH") {
            this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerInject.viewContainerRef, {
                title: 'Confirm Remove Invoice',
                body: 'Some Debit Values will change, do you want to remove you selections?',
                labelConfirm: 'Yes',
                labelCancel: 'No'
            }, () => {
                this.onDeleteInvoiceItem();
            })
        } else if (!!item.id) {
            this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerInject.viewContainerRef, {
                title: 'Confirm Remove Invoice',
                body: 'Some Debit Values will change, do you want to remove you selections?',
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
        if (this.selectedIndexItem === undefined) {
            return;
        }
        this._store.dispatch(RemoveInvoice({ index: this.selectedIndexItem }));
        this.onChangeDebit.emit(true);
    }

    calculateTotalPaidAmount(item: ReceiptInvoiceModel, type: string) {
        switch (type) {
            case 'paidVnd':
                if (!!item.creditNo) {
                    item.totalPaidVnd = +item.paidAmountVnd + (item.creditAmountVnd ?? 0);
                } else {
                    if (!!item.exchangeRateBilling) {
                        item.totalPaidUsd = item.paidAmountUsd = +((item.paidAmountVnd / item.exchangeRateBilling).toFixed(2));
                    }
                    item.totalPaidVnd = +item.paidAmountVnd;
                }
                break;
            case 'paidUsd':
                if (item.creditNo) {
                    item.totalPaidUsd = +item.paidAmountUsd + (item.creditAmountUsd ?? 0);
                } else {
                    //* [16056]
                    if (!!item.exchangeRateBilling) {
                        item.totalPaidVnd = item.paidAmountVnd = +(item.paidAmountUsd * item.exchangeRateBilling);

                    }
                    item.totalPaidUsd = +item.paidAmountUsd;
                }
                break;
            default:

                break;
        }

        this.debitList$.pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (debitList: ReceiptInvoiceModel[]) => {
                    if (!!debitList.length) {
                        this.sumTotalObj = this.calculateTotal(debitList);
                    }
                }
            )
    }

    onChangeCalCredit(_refNo: string, curr: ReceiptInvoiceModel) {
        if (!!_refNo) {
            this.creditList$
                .pipe(takeUntil(this.ngUnsubscribe))
                .subscribe(
                    (credits: ReceiptInvoiceModel[]) => {
                        if (!!credits.length) {
                            const currentCredit = credits.find(x => x.refNo == _refNo);
                            if (!!currentCredit) {
                                if (currentCredit.amount > curr.amount) {
                                    this._toastService.warning("Credit Amount > Invoice Amount", "Warning");
                                    curr.creditNo = null;
                                    return;
                                }
                                curr.creditNo = _refNo;

                                curr.creditAmountVnd = currentCredit.unpaidAmountVnd;
                                curr.creditAmountUsd = currentCredit.unpaidAmountUsd;

                                curr.paidAmountVnd = curr.unpaidAmountVnd - currentCredit?.unpaidAmountVnd;
                                curr.paidAmountUsd = curr.unpaidAmountUsd - currentCredit?.unpaidAmountUsd;

                                curr.totalPaidVnd = currentCredit?.unpaidAmountVnd + curr.paidAmountVnd;
                                curr.totalPaidUsd = currentCredit?.unpaidAmountUsd + curr.paidAmountUsd;

                                this._dataService.setData('clearCredit', { invoiceNo: curr.invoiceNo, creditNo: _refNo });
                            }
                        }
                    }
                )
        }
    }

    calculateTotal(model: ReceiptInvoiceModel[]) {
        const totalData = {
            totalUnpaidAmountUsd: 0,
            totalUnpaidAmountVnd: 0,
            totalPaidAmountVnd: 0,
            totalPaidAmountUsd: 0,
            totalPaidVnd: 0,
            totalPaidUsd: 0,
            totalRemainVnd: 0,
            totalRemainUsd: 0
        };

        for (let index = 0; index < model.length; index++) {
            const item: ReceiptInvoiceModel = model[index];
            if (model[index].type !== 'CREDIT') {
                totalData.totalUnpaidAmountUsd += (+item.unpaidAmountUsd ?? 0);
                totalData.totalUnpaidAmountVnd += (+item.unpaidAmountVnd ?? 0);
                totalData.totalPaidAmountUsd += (+item.paidAmountUsd ?? 0);
                totalData.totalPaidAmountVnd += (+item.paidAmountVnd ?? 0);
                totalData.totalPaidUsd += (+item.totalPaidUsd ?? 0);
                totalData.totalPaidVnd += (+item.totalPaidVnd ?? 0);
                totalData.totalRemainUsd = (+totalData.totalUnpaidAmountUsd ?? 0) - (+totalData.totalPaidUsd ?? 0);
                totalData.totalRemainVnd = (totalData.totalUnpaidAmountVnd ?? 0) - (+totalData.totalPaidVnd ?? 0);
            }

        }


        return totalData
    }
}
