import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { ICDCombine, IReceiptCombineGroup, ReceiptInvoiceModel, ReceiptModel } from '@models';
import { Store, ActionsSubject } from '@ngrx/store';
import { CatalogueRepo, SystemRepo, AccountingRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { takeUntil } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { AgencyReceiptModel } from 'src/app/shared/models/accouting/agency-receipt.model';
import { GetInvoiceListSuccess, RegistDebitListTypeReceipt, RegistGetDebitType, RemoveDebitCombine } from '../../store/actions';
import { ICustomerPaymentState, ReceiptCombineCreditInvoiceState, ReceiptCombineDebitInvoiceState, ReceiptCombineDeditListState, ReceiptCombineExchangeState, ReceiptCombineLoadingState, ReceiptCombinePartnerState, ReceiptCombineState } from '../../store/reducers';
import { ARCustomerPaymentCustomerAgentDebitPopupComponent } from '../customer-agent-debit/customer-agent-debit.popup';

@Component({
    selector: 'receipt-cd-combine',
    templateUrl: './receipt-cd-combine.component.html',
})
export class ARCustomerPaymentReceiptCDCombineComponent extends AppList implements OnInit {
    @ViewChild(ARCustomerPaymentCustomerAgentDebitPopupComponent) debitPopup: ARCustomerPaymentCustomerAgentDebitPopupComponent;

    @Input() listType: string;
    @Input() isUpdate: boolean = false;
    @Output() onSaveReceipt: EventEmitter<Partial<any>> = new EventEmitter<Partial<any>>();

    cdCombineList: ICDCombine[] = [];
    paymentMethodsCredit: CommonInterface.ICommonTitleValue[] = [
        { title: 'Clear Credit From OBH', value: 'Clear Credit From OBH' },
        { title: 'Clear Credit From Paid AMT', value: 'Clear Credit From Paid AMT' }
    ];
    paymentMethodsDebit: CommonInterface.ICommonTitleValue[] = [
        { title: 'Clear Debit From OBH', value: 'Clear Debit From OBH' },
        { title: 'Clear Debit From Paid AMT', value: 'Clear Debit From Paid AMT' }
    ];

    exchangeRate = this._store.select(ReceiptCombineExchangeState);
    selectedIndexItem: number;
    isSubmitted: boolean = false;
    paymentList: ReceiptInvoiceModel[] = [];
    arcbno: string = '';
    isContainDraftCredit: boolean = false;
    isContainDraftDebit: boolean = false;
    
    receiptCreditGroups: IReceiptCombineGroup[] = [];
    receiptDebitGroups: IReceiptCombineGroup[] = [];

    sumTotalCredit = {
        totalUnpaidAmountUsd: 0,
        totalUnpaidAmountVnd: 0,
        totalPaidAmountVnd: 0,
        totalPaidAmountUsd: 0,
        totalRemainVnd: 0,
        totalRemainUsd: 0
    };
    sumTotalDebit = {
        totalUnpaidAmountUsd: 0,
        totalUnpaidAmountVnd: 0,
        totalPaidAmountVnd: 0,
        totalPaidAmountUsd: 0,
        totalRemainVnd: 0,
        totalRemainUsd: 0
    };

    constructor(
        private readonly _store: Store<ICustomerPaymentState>,
        private readonly _catalogueRepo: CatalogueRepo,
        private readonly _systemRepo: SystemRepo,
        private readonly _actionStoreSubject: ActionsSubject,
        private readonly _accountingRepo: AccountingRepo,
        private readonly _toastService: ToastrService,
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
            { title: 'Note', field: '', width: 200 },
            { title: 'Unpaid Local', field: '' },
            { title: 'Amount Local', field: '' },
            { title: 'Remain VND', field: '' },
            { title: 'Invoice No', field: '' },
            { title: 'Acct Ref', field: '' },
        ];

        // this._store.select(ReceiptCombineExchangeState)
        //     .pipe(takeUntil(this.ngUnsubscribe))
        //     .subscribe((exchange: number) => { this.exchangeRate = exchange });
        this._store.select(ReceiptCombinePartnerState)
        .pipe(takeUntil(this.ngUnsubscribe))
        .subscribe(
            (id) => {
                this._store.dispatch(RegistGetDebitType({
                    listType: this.listType,
                    partnerId: id,
                    officeId: ''
                }));
            }
        )
    }

    getDebit() {
        this._store.dispatch(RegistDebitListTypeReceipt({
            listType: this.listType
        }));
        if (this.listType === 'debit') {
            this._store.select(ReceiptCombineDebitInvoiceState)
                .subscribe((data: any) => {
                    if (!!data) {
                        this.debitPopup.agencyDebitModel = data;
                    }
                    else {
                        this.debitPopup.agencyDebitModel = new AgencyReceiptModel();
                    }
                    this.debitPopup.show();
                })
        } else {
            this._store.select(ReceiptCombineCreditInvoiceState)
                .subscribe((data: any) => {
                    if (!!data) {
                        this.debitPopup.agencyDebitModel = data;
                    } else {
                        this.debitPopup.agencyDebitModel = new AgencyReceiptModel();
                    }
                    this.debitPopup.show();
                })
        }
        //     // if (!!this.date.value?.startDate) {
        //     //     this._store.dispatch(SelectReceiptDate({ date: this.date.value }));
        //     // }
    }

    getDebitDetail(item: any, _type: string) {
        this._store.dispatch(RegistGetDebitType({
            listType: _type,
            partnerId: item.partnerId,
            officeId: item.officeId
        }));
        this._store.dispatch(GetInvoiceListSuccess({ invoices: item.cdCombineList }));
        this.debitPopup.show();
    }

    updateInvoiceList(data: any, type: string) {
        if (type === 'debit') {
            this.cdCombineList = data;
            // this.calculateTotal(this.cdCombineList);
        } else {
            this.cdCombineList = data;
            // this.calculateTotal(this.cdCombineList);
        }
    }

    calculateTotal(model: IReceiptCombineGroup) {
        if(!model){
            return;
        }
        const totalData = {
            totalUnpaidAmountUsd: 0,
            totalUnpaidAmountVnd: 0,
            totalPaidAmountVnd: 0,
            totalPaidAmountUsd: 0,
            totalRemainVnd: 0,
            totalRemainUsd: 0,
        };

        for (let index = 0; index < model.cdCombineList.length; index++) {
            const item: ICDCombine = model.cdCombineList[index];
            totalData.totalUnpaidAmountUsd += (+(item.unpaidAmountUsd) ?? 0);
            totalData.totalUnpaidAmountVnd += (+(item.unpaidAmountVnd) ?? 0);
            totalData.totalPaidAmountUsd += (+(item.paidAmountUsd) ?? 0);
            totalData.totalPaidAmountVnd += (+(item.paidAmountVnd) ?? 0);
            totalData.totalRemainUsd = (+(item.remainAmountUsd) ?? 0);
            totalData.totalRemainVnd = (+(item.remainAmountVnd) ?? 0);
        }
        model.sumTotal = totalData;
        return model;
    }

    calculateTotalPaidAmount(item: ICDCombine, model: IReceiptCombineGroup) {
        item.remainAmountUsd = item.unpaidAmountUsd - item.paidAmountUsd;
        item.paidAmountVnd = item.currencyId === 'USD' ? +((+item.paidAmountUsd * (item.exchangeRateBilling ?? 0)).toFixed(2)) : item.paidAmountVnd;
        item.remainAmountVnd = item.unpaidAmountVnd - item.paidAmountVnd;
        model = this.calculateTotal(model);
    }

    confirmDeleteInvoiceItem(item: IReceiptCombineGroup, index: number, type: string) {
        this.selectedIndexItem = index;
        this.onDeleteInvoiceItem(item, index, type);
        
    }

    onDeleteInvoiceItem(item: any, index: number, type: string) {
        if (this.selectedIndexItem === undefined) {
            return;
        }
        this._store.dispatch(RemoveDebitCombine({ index: this.selectedIndexItem, _typeList: type }));
        this.calculateTotal(item);
    }

    onSaveReceiptGroup(type: string, action: string, group: any = null){
        this.isSubmitted = true;
        const data = {
            type: type,
            action: action,
            receipt: group
        }
        this.onSaveReceipt.emit(data);
    }
}