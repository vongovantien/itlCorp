import { OnInit, Component, ChangeDetectionStrategy, EventEmitter, Output, Input, ViewChild } from "@angular/core";
import { AppList } from "@app";
import { Store } from "@ngrx/store";
import { ReceiptInvoiceModel } from "@models";
import { ConfirmPopupComponent } from "@common";
import { InjectViewContainerRefDirective } from "@directives";

import { IReceiptState } from "../../store/reducers/customer-payment.reducer";
import { ReceiptDebitListState, ReceiptTypeState, ReceiptCreditListState, ReceiptIsAutoConvertPaidState } from "../../store/reducers";
import { RemoveInvoice, ChangeADVType, InsertCreditToDebit, UpdateCreditItemValue } from "../../store/actions";

import { takeUntil } from "rxjs/operators";
import { Observable } from "rxjs";

@Component({
    selector: 'customer-payment-receipt-debit-list',
    templateUrl: './receipt-debit-list.component.html',
    styleUrls: ['./receipt-debit-list.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ARCustomerPaymentReceiptDebitListComponent extends AppList implements OnInit {
    @ViewChild(InjectViewContainerRefDirective) viewContainerInject: InjectViewContainerRefDirective;
    @Input() isReadonly: boolean = false;

    debitList$ = this._store.select(ReceiptDebitListState);
    creditList$ = this._store.select(ReceiptCreditListState);

    agencyHeaders: CommonInterface.IHeaderTable[] = [
        { title: 'RefNo', field: '', sortable: true },
        { title: 'Type', field: '', width: 150 },
        { title: 'Invoice No', field: '' },
        { title: 'Credit No', field: '', width: 300 },
        { title: 'Unpaid USD', field: '', width: 150, align: this.right },
        { title: 'Unpaid VND', field: '', width: 150, align: this.right },
        { title: 'Paid Amount USD', field: '', width: 150, align: this.right, required: true, },
        { title: 'Paid Amount VND', field: '', width: 150, align: this.right, required: true },
        { title: 'Total Paid USD', field: '', width: 150, align: this.right },
        { title: 'Total Paid VND', field: '', width: 150, align: this.right },
        { title: 'Remain USD', field: '', width: 150, align: this.right },
        { title: 'Remain VND', field: '', width: 150, align: this.right },
        { title: 'Note', field: '', width: 200 },
        { title: 'BU Handle', field: '' },
        { title: 'Office', field: '' },
        { title: 'NetOff Only', field: '', width: 150 },
        { title: 'Job No', field: '', width: 150 },
        { title: 'MBL No', field: '', width: 150 },
        { title: 'HBL No', field: '', width: 150 },

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

    isAutoConvert: boolean;
    selectedPaymentItemIndex: number = -1;

    constructor(
        private readonly _store: Store<IReceiptState>,
    ) {
        super();
    }

    ngOnInit() {
        this.headers = [
            { title: 'RefNo', field: '', sortable: true, width: 100 },
            { title: 'Type', field: '', width: 100 },
            { title: 'Invoice No', field: '', width: 100 },
            { title: 'Credit No', field: '', width: 180 },
            { title: 'Unpaid USD', field: '', width: 150, align: this.right },
            { title: 'Unpaid VND', field: '', width: 150, align: this.right },
            { title: 'Paid Amount USD', field: '', width: 150, align: this.right, required: true },
            { title: 'Paid Amount VND', field: '', width: 150, align: this.right, required: true },
            { title: 'Total Paid USD', field: '', width: 150, align: this.right },
            { title: 'Total Paid VND', field: '', width: 150, align: this.right },
            { title: 'Remain USD', field: '', width: 150, align: this.right },
            { title: 'Remain VND', field: '', width: 150, align: this.right },
            { title: 'Note', field: '', width: 200 },
            { title: 'BU Handle', field: '' },
            { title: 'Office', field: '' },
            { title: 'NetOff Only', field: '', width: 150 }
        ];


        this.calculateSumTotalDebit();

        this._store.select(ReceiptIsAutoConvertPaidState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (isAutoConvert) => {
                    this.isAutoConvert = isAutoConvert;
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
    }

    calculateTotalPaidAmount(item: ReceiptInvoiceModel, type: string) {
        item.isChangeValue = true; // ! Detect user changed -> Flag to Force Process Clear
        switch (type) {
            case 'paidVnd':
                // if (!!item.creditNo) {
                //     item.totalPaidVnd = +item.paidAmountVnd + (item.creditAmountVnd ?? 0);
                // } else {
                //     if (!!item.exchangeRateBilling && !!this.isAutoConvert) {
                //         item.totalPaidUsd = item.paidAmountUsd = +((item.paidAmountVnd / item.exchangeRateBilling).toFixed(2));
                //     }
                //     item.totalPaidVnd = +item.paidAmountVnd;
                // }
                if (!!item.exchangeRateBilling && !!this.isAutoConvert) {
                    item.totalPaidUsd = item.paidAmountUsd = +((item.paidAmountVnd / item.exchangeRateBilling).toFixed(2));
                }
                item.totalPaidVnd = +item.paidAmountVnd;
                break;
            case 'paidUsd':
                // if (item.creditNo) {
                //     item.totalPaidUsd = +item.paidAmountUsd + (item.creditAmountUsd ?? 0);
                // } else {
                //     //* [16056]
                //     if (!!item.exchangeRateBilling && !!this.isAutoConvert) {
                //         item.totalPaidVnd = item.paidAmountVnd = +(item.paidAmountUsd * item.exchangeRateBilling);
                //     }
                //     item.totalPaidUsd = +item.paidAmountUsd;
                // }
                //* [16056]
                if (!!item.exchangeRateBilling && !!this.isAutoConvert) {
                    item.totalPaidVnd = item.paidAmountVnd = +(item.paidAmountUsd * item.exchangeRateBilling);
                }
                item.totalPaidUsd = +item.paidAmountUsd;
                break;
            default:

                break;
        }
        this.calculateSumTotalDebit();

    }

    calculateSumTotalDebit() {
        this.debitList$.pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (debitList: ReceiptInvoiceModel[]) => {
                    if (!!debitList.length) {
                        this.sumTotalObj = this.calculateTotal(debitList);
                    }
                }
            )
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
            if (model[index].type !== 'CREDIT' && model[index].negative !== true) {
                totalData.totalUnpaidAmountUsd += (+(item.unpaidAmountUsd) ?? 0);
                totalData.totalUnpaidAmountVnd += (+(item.unpaidAmountVnd) ?? 0);
                totalData.totalPaidAmountUsd += (+(item.paidAmountUsd) ?? 0);
                totalData.totalPaidAmountVnd += (+(item.paidAmountVnd) ?? 0);
                totalData.totalPaidUsd += (+(item.totalPaidUsd) ?? 0);
                totalData.totalPaidVnd += (+(item.totalPaidVnd) ?? 0);
                totalData.totalRemainUsd = (+(totalData.totalUnpaidAmountUsd) ?? 0) - (+(totalData.totalPaidUsd) ?? 0);
                totalData.totalRemainVnd = (+(totalData.totalUnpaidAmountVnd) ?? 0) - (+(totalData.totalPaidVnd) ?? 0);
            }
        }
        return totalData
    }

    selectPaymentItem(index: number) {
        this.selectedPaymentItemIndex = index;
        console.log(this.selectedPaymentItemIndex);
    }

    changePaymentTypeADV(newADVType: string) {
        if (this.selectedPaymentItemIndex >= 0) {
            this._store.dispatch(ChangeADVType({ index: this.selectedPaymentItemIndex, newType: newADVType }));
        }
    }

    selectCreditPaymentItem(item: ReceiptInvoiceModel) {
        if (this.selectedPaymentItemIndex >= 0) {
            this._store.dispatch(InsertCreditToDebit(
                {
                    index: this.selectedPaymentItemIndex,
                    creditNo: item.refNo,
                    creditAmountVnd: item.paidAmountVnd,
                    creditAmountUsd: item.paidAmountUsd
                }
            ));

        }
    }

    deleteCreditItem(credit: string, item: ReceiptInvoiceModel, e: Event) {
        e.preventDefault();
        e.stopImmediatePropagation();

        const index = (item.creditNos as string[]).findIndex(x => x === credit);

        item.creditNos = [...item.creditNos.slice(0, index), ...item.creditNos.slice(index + 1)];

        this._store.dispatch(UpdateCreditItemValue({ searchKey: 'refNo', searchValue: credit, key: 'invoiceNo', value: null }));

    }

    onToggleNetOfOnly(isNetOff: boolean, item: ReceiptInvoiceModel) {

        let totalNetOffVNd: number = 0;
        let totalNetOffUsd: number = 0;
        let creditMapPriceValue: ICreditNetOffMapValue[];
        item.netOff = isNetOff;
        if (!!item.creditNos.length) {
            this.creditList$
                .pipe(takeUntil(this.ngUnsubscribe))
                .subscribe((creditList: ReceiptInvoiceModel[]) => {
                    creditMapPriceValue = creditList.map((item: ReceiptInvoiceModel) =>
                        ({ creditNo: item.refNo, netOffVnd: item.paidAmountVnd, netOffUsd: item.paidAmountUsd })
                    )
                    item.creditNos.forEach((credit: string) => {
                        const currentCreditMapping = creditMapPriceValue.find(x => x.creditNo == credit);
                        if (!!currentCreditMapping) {
                            totalNetOffVNd += currentCreditMapping.netOffVnd;
                            totalNetOffUsd += currentCreditMapping.netOffUsd;
                        }
                    })

                    if (isNetOff === true) {
                        item.paidAmountVnd = item.totalPaidVnd = totalNetOffVNd;
                        item.paidAmountUsd = item.totalPaidUsd = totalNetOffUsd;
                    }
                    else {
                        // item.paidAmountVnd = item.unpaidAmountVnd - totalNetOffVNd;
                        // item.paidAmountUsd = item.unpaidAmountUsd - totalNetOffUsd;
                    }
                    this.calculateSumTotalDebit();

                })
        }
    }

}

interface ICreditNetOffMapValue {
    creditNo: string;
    netOffUsd: number;
    netOffVnd: number;
}