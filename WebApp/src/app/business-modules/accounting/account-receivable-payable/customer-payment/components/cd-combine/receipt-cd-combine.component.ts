import { Component, Input, OnInit } from '@angular/core';
import { Store, ActionsSubject } from '@ngrx/store';
import { CatalogueRepo, SystemRepo, AccountingRepo } from '@repositories';
import { AppList } from 'src/app/app.list';
import { ICustomerPaymentState, ReceiptCombineLoadingState } from '../../store/reducers';

@Component({
    selector: 'receipt-cd-combine',
    templateUrl: './receipt-cd-combine.component.html',
})
export class ARCustomerPaymentReceiptCDCombineComponent extends AppList implements OnInit {
    @Input() type: string;
    cdCombineList: ICDCombine[] = [{
        partnerName: 'FLEXPORT',
        billingNo: 'billingNo',
        unpaidAmount: 100,
        unpaidAmountVnd: 100,
        unpaidAmountUsd: 100,
        hbl: 'HBL',
        mbl: 'MBL',
        remain: 0,
        remainVnd: 0,
        remainUsd: 0,
        invoiceNo: "INV",
        refNo: 'REF#133'
    }];

    paymentMethods: CommonInterface.ICommonTitleValue[] = [
        { title: 'Clear Credit from OBH', value: 'Clear Credit from OBH' },
        { title: 'Clear Credit from OBH', value: 'Clear Credit from Paid AMT' }
    ];

    constructor(
        private readonly _store: Store<ICustomerPaymentState>,
        private readonly _catalogueRepo: CatalogueRepo,
        private readonly _systemRepo: SystemRepo,
        private readonly _actionStoreSubject: ActionsSubject,
        private readonly _accountingRepo: AccountingRepo,
    ) {
        super();
    }

    ngOnInit(): void {
        this.isLoading = this._store.select(ReceiptCombineLoadingState);
        this.headers = [
            { title: 'Agency Name', field: '', width: 200, },
            { title: 'Billing No', field: '' },
            { title: 'Unpaid', field: '' },
            { title: 'Amount', field: '', width: 200, required: true },
            { title: 'Remain', field: '' },
            { title: 'HBL - MBL', field: '' },
            { title: 'Note', field: '' },
            { title: 'Unpaid Local', field: '' },
            { title: 'Amount Local', field: '' },
            { title: 'Remain VND', field: '' },
            { title: 'Invoice No', field: '' },
            { title: 'Acct Ref', field: '' },
        ];

        console.log(this.cdCombineList);
    }

    deleteCdCombineItem(index: number) {

    }
}

interface ICDCombine {
    partnerId?: string,
    partnerName?: string;
    billingNo?: string;
    amount?: number;
    amountVnd?: number;
    amountUsd?: number;
    unpaidAmount?: number;
    unpaidAmountVnd?: number;
    unpaidAmountUsd?: number;
    remain?: number;
    remainVnd?: number;
    remainUsd?: number;
    hbl?: string;
    hblId?: string;
    mbl?: string;
    notes?: string;
    invoiceNo?: string;
    accMngtId?: string;
    voucherId?: string;
    refNo?: string;
    [key: string]: any;
}
