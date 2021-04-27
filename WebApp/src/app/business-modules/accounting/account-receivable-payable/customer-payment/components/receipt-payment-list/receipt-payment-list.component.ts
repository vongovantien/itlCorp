import { Component, OnInit, Input, TemplateRef, ChangeDetectionStrategy, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { ReceiptInvoiceModel, Currency, Partner } from '@models';
import { NgProgress } from '@ngx-progressbar/core';
import { AccountingRepo, CatalogueRepo } from '@repositories';
import { SortService, DataService } from '@services';
import { formatCurrency, formatDate } from '@angular/common';
import { AppList } from '@app';
import { FormGroup, FormBuilder, AbstractControl, Validators } from '@angular/forms';
import { IAppState, getCatalogueCurrencyState, GetCatalogueCurrencyAction, getCurrentUserState } from '@store';

import { Store } from '@ngrx/store';
import { takeUntil, pluck } from 'rxjs/operators';
import { Observable, from, BehaviorSubject } from 'rxjs';
import { customerPaymentReceipInvoiceListState, customerPaymentReceipLoadingState, ReceiptCreditListState, ReceiptDebitListState } from '../../store/reducers';
import { ToastrService } from 'ngx-toastr';
import { InsertAdvance } from '../../store/actions';
import { ARCustomerPaymentReceiptDebitListComponent } from '../receipt-debit-list/receipt-debit-list.component';
import { ARCustomerPaymentReceiptCreditListComponent } from '../receipt-credit-list/receipt-credit-list.component';
import { ConfirmPopupComponent } from '@common';

@Component({
    selector: 'customer-payment-list-receipt',
    templateUrl: './receipt-payment-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})

export class ARCustomerPaymentReceiptPaymentListComponent extends AppList implements OnInit {
    @ViewChild(ARCustomerPaymentReceiptDebitListComponent) receiptDebitList: ARCustomerPaymentReceiptDebitListComponent;
    @ViewChild(ARCustomerPaymentReceiptCreditListComponent) receiptCreditList: ARCustomerPaymentReceiptCreditListComponent;
    @ViewChild(ConfirmPopupComponent) confirmPopup: ConfirmPopupComponent;
    
    @Input() syncInfoTemplate: TemplateRef<any>

    invoices: ReceiptInvoiceModel[] = [];
    creditList: Observable<ReceiptInvoiceModel[]>;
    debitList: Observable<ReceiptInvoiceModel[]>;
    term$ = new BehaviorSubject<string>('');
    
    form: FormGroup;
    paidAmount: AbstractControl;
    methods: CommonInterface.ICommonTitleValue[];
    userLogged: Partial<SystemInterface.IClaimUser>;
    type: AbstractControl;
    cusAdvanceAmount: AbstractControl;
    finalPaidAmount: AbstractControl;
    balance: AbstractControl;
    paymentMethod: AbstractControl;
    currencyId: AbstractControl;
    paymentDate: AbstractControl;
    exchangeRate: AbstractControl;
    bankAccountNo: AbstractControl;
    description: AbstractControl;
    amountVND: AbstractControl;
    amountUSD: AbstractControl;
    paidAmountVND: AbstractControl;
    paidAmountUSD: AbstractControl;
    finalPaidAmountVND: AbstractControl;
    finalPaidAmountUSD: AbstractControl;

    $currencyList: Observable<Currency[]>;


    paymentMethods: string[] = ['Cash', 'Bank Transfer', 'Other'];
    receiptTypes: string[] = ['Debit', 'NetOff Adv'];

    customerInfo: Partner = null;

    isSubmitted: boolean = false;
    isReadonly: boolean = null;  // * DONE | CANCEL
    exchangeRateUsd: number = 1;

    headerReceiptReadonly: CommonInterface.IHeaderTable[] = [
        { title: 'Billing Ref No', field: 'invoiceNo' },
        { title: 'Series No', field: 'serieNo' },
        { title: 'Type', field: 'type' },
        { title: 'Partner Name', field: 'partnerName' },
        { title: 'Taxcode', field: 'taxCode' },
        // { title: 'Unpaid Amount', field: 'unpaidAmount' },
        { title: 'Paid Amount', field: 'paidAmount' },
        { title: 'Balance Amount', field: 'invoiceBalance' },
        { title: 'Payment Status', field: 'paymentStatus' },
        { title: 'Billing Date', field: 'billingDate' },
        { title: 'Invoice Date', field: 'invoiceDate' },
        { title: 'Note', field: 'note' },
    ];

    constructor(
        private _sortService: SortService,
        private _progressService: NgProgress,
        private _router: Router,
        private _accountingRepo: AccountingRepo,
        private _store: Store<IAppState>,
        private _fb: FormBuilder,
        private _dataService: DataService,
        private _catalogueRepo: CatalogueRepo,
        private _toastService: ToastrService,
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestSort = this.sortTrialOfficalList;
    }

    ngOnInit() {
        this._store.dispatch(new GetCatalogueCurrencyAction());
        this.$currencyList = this._store.select(getCatalogueCurrencyState);

        this.initForm();

        this.headers = [
            { title: 'Billing Ref No', field: 'invoiceNo', sortable: true },
            { title: 'Series No', field: 'serieNo', sortable: true },
            { title: 'Type', field: 'type', sortable: true },
            { title: 'Partner Name', field: 'partnerName', sortable: true },
            { title: 'Taxcode', field: 'taxCode', sortable: true },
            { title: 'Unpaid Amount', field: 'unpaidAmount', sortable: true },
            { title: 'Unpaid Ex Amount', field: 'receiptExcUnpaidAmount', sortable: true },
            { title: 'Paid Amount', field: 'paidAmount', sortable: true },
            { title: 'Paid Ex Amount', field: 'receiptExcPaidAmount', sortable: true },
            { title: 'Balance Amount', field: 'invoiceBalance', sortable: true },
            { title: 'Balance Ex Amount', field: 'receiptInvoicebalance', sortable: true },
            { title: 'Payment Status', field: 'paymentStatus', sortable: true },
            { title: 'Billing Date', field: 'billingDate', sortable: true },
            { title: 'Invoice Date', field: 'invoiceDate', sortable: true },
            { title: 'Note', field: 'note', sortable: true },
        ];

        this.isLoading = this._store.select(customerPaymentReceipLoadingState);
        this.initSubscriptioontInvoiceList();
        this.listenCusAdvanceData();
        this.listenCustomerInfoData();
        this._store.select(getCurrentUserState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((u) => {
                if (!!u) {
                    this.userLogged = u;
                }
            });
        this.generateExchangeRateUSD(formatDate(this.paymentDate.value?.startDate, 'yyy-MM-dd', 'en'));
        this.creditList = this._store.select(ReceiptCreditListState);
        this.debitList = this._store.select(ReceiptDebitListState);
    }

    formatNumberCurrency(input: number, digit: number) {
        return input.toLocaleString(
            'en-US', // leave undefined to use the browser's locale, or use a string like 'en-US' to override it.
            { minimumFractionDigits: digit }
        );
    }

    initSubscriptioontInvoiceList() {
        this._store.select(customerPaymentReceipInvoiceListState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (res: ReceiptInvoiceModel[]) => {
                    this.invoices = [...res];
                    this.balance.setValue(null);

                    console.log(this.invoices);
                });
    }

    listenCusAdvanceData() {
        this._dataService.currentMessage
            .pipe(pluck('cus-advance'))
            .subscribe(
                (data) => {
                    data !== undefined && !this.cusAdvanceAmount.value && this.cusAdvanceAmount.setValue(data);
                }
            );
    }

    listenCustomerInfoData() {
        this._dataService.currentMessage
            .pipe(pluck('customer'))
            .subscribe(
                (data: Partner) => {
                    data !== undefined && (this.customerInfo = data);
                }
            );
    }

    listenCurrencyInfoData() {
        this._dataService.currentMessage
            .pipe(pluck('currency'))
            .subscribe(
                (data: Partner) => {
                    data !== undefined&& !this.currencyId.value && (this.currencyId.setValue(data));
                }
            );
    }

    initForm() {
        this.form = this._fb.group({
            paidAmount: [null, Validators.required],
            type: [[this.receiptTypes[0]]],
            cusAdvanceAmount: [],
            finalPaidAmount: [{ value: null, disabled: true }],
            balance: [{ value: null, disabled: true }],
            paymentMethod: [this.paymentMethods[0]],
            currencyId: ['VND'],
            paymentDate: [{ startDate: new Date(), endDate: new Date() }],
            exchangeRate: [1, Validators.required],
            bankAccountNo: [],
            amountVND: [],
            amountUSD: [],
            paidAmountVND: [],
            paidAmountUSD: [],
            description: [],
            finalPaidAmountVND: [],
            finalPaidAmountUSD: []
        });

        this.paidAmount = this.form.controls['paidAmount'];
        this.type = this.form.controls['type'];
        this.cusAdvanceAmount = this.form.controls['cusAdvanceAmount'];
        this.finalPaidAmount = this.form.controls['finalPaidAmount'];
        this.balance = this.form.controls['balance'];
        this.paymentMethod = this.form.controls['paymentMethod'];
        this.paymentDate = this.form.controls['paymentDate'];
        this.exchangeRate = this.form.controls['exchangeRate'];
        this.bankAccountNo = this.form.controls['bankAccountNo'];
        this.currencyId = this.form.controls['currencyId'];
        this.description = this.form.controls['description'];
        this.amountVND = this.form.controls['amountVND'];
        this.amountUSD = this.form.controls['amountUSD'];
        this.paidAmountVND = this.form.controls['paidAmountVND'];
        this.paidAmountUSD = this.form.controls['paidAmountUSD'];
        this.finalPaidAmountVND = this.form.controls['finalPaidAmountVND'];
        this.finalPaidAmountUSD = this.form.controls['finalPaidAmountUSD'];
    }

    generateExchangeRateUSD(date: string) {
        this._catalogueRepo.convertExchangeRate(date, 'USD')
            .pipe()
            .subscribe(
                (data: { rate: number }) => {
                    this.exchangeRateUsd = data?.rate;
                }
            );
    }

    getBankAccountNo(event: any){
        if(event === this.paymentMethods[1]){
            if(this.currencyId.value === 'VND'){
                this.bankAccountNo.setValue(this.userLogged.bankOfficeAccountNoVnd)
            }else{
                this.bankAccountNo.setValue(this.userLogged.bankOfficeAccountNoUsd)
            }
        }else{
            this.bankAccountNo.setValue(null);
        }
    }

    sortTrialOfficalList(sortField: string, order: boolean) {
        this.invoices = this._sortService.sort(this.invoices, sortField, order);
    }

    onChangeCheckBoxAction() {
        for (const charge of this.invoices) {
            charge.isSelected = this.isCheckAll;
        }
    }

    onChangeCheckBoxInvoice() {
        this.isCheckAll = this.invoices.every(x => x.isSelected);
    }

    onSelectDataFormInfo(data, type: string) {
        switch (type) {
            case 'paid-amount':
                if (this.type.value?.length === 1) {
                    if (this.type.value?.includes(this.receiptTypes[0])) {
                        this.finalPaidAmount.setValue(data);
                    } else if (this.type.value?.includes(this.receiptTypes[1])) {
                        this.finalPaidAmount.setValue(+this.cusAdvanceAmount.value);
                    }
                } else {
                    this.finalPaidAmount.setValue(this.cusAdvanceAmount.value + +this.paidAmount.value);
                }

                break;
            case 'paid-amountVnd':
                this.paidAmountUSD.setValue(this.formatNumberCurrency(this.paidAmountVND.value / this.exchangeRateUsd, 0));
                break;
            case 'paid-amountUsd':
                    this.paidAmountVND.setValue(this.formatNumberCurrency(Math.round(this.paidAmountUSD.value * this.exchangeRateUsd), 2));
                    break;
            case 'type':
                if (data.length === 1) {
                    if (data?.includes(this.receiptTypes[0])) {
                        this.finalPaidAmount.setValue(+this.paidAmount.value);
                    } else if (data.includes(this.receiptTypes[1])) {
                        this.finalPaidAmount.setValue(+this.cusAdvanceAmount.value);
                    }
                } else {
                    this.finalPaidAmount.setValue(this.cusAdvanceAmount.value + +this.paidAmount.value);
                }
                break;
            case 'currency':
                if ((data as Currency).id === this.getCurrencyInvoice(this.invoices)[0]) {
                    this.exchangeRate.setValue(1);
                    break;
                } else {
                    if ((data as Currency).id !== 'VND') {
                        this.exchangeRate.setValue(this.exchangeRateUsd);
                    }else{
                        this.exchangeRate.setValue(1);
                    }

                }

                this.balance.setValue(null);
                this.finalPaidAmount.setValue(null);
                this.paidAmount.setValue(null);
                break;

            case 'payment-date':
                this.generateExchangeRateUSD(formatDate(data?.startDate, 'yyy-MM-dd', 'en'));
                if (this.currencyId.value === 'VND') {
                    this.exchangeRate.setValue(1);
                }else{
                    this.exchangeRate.setValue(this.exchangeRateUsd);
                }
                break;
            default:
                break;
        }
    }

    onSearchAutoComplete(keyword: string) {
        console.log('text2', keyword)
        // if(keyword === 'paid-amountVnd'){
        //     this.paidAmountUSD.setValue(this.paidAmountVND.value / this.exchangeRateUsd);
        // }else{
        //     this.paidAmountVND.setValue(this.paidAmountUSD.value * this.exchangeRate.value);
        // }
        // this.isLoading = false;
    }

    // autocomplete = (time: number, callBack: Function) => (source$: Observable<any>) =>
    //     source$.pipe(
    //         debounceTime(time),
    //         distinctUntilChanged(),
    //         tap(() => {
    //             this.isLoading = true;
    //         }),
    //         switchMap((...args: any[]) => callBack(...args).pipe(
    //             takeUntil(source$.pipe(skip(1)))
    //         )
    //         )
    //     )
    
    showConfirmPopup(){
        this.confirmPopup.show();
    }

    removeReceiptItem() {
        this.confirmPopup.hide();
        this.receiptDebitList.removeListItem();
        this.receiptCreditList.removeListItem();
    }
    insertAdvanceRowData() {
        const newInvoiceWithAdv: ReceiptInvoiceModel = new ReceiptInvoiceModel({
            typeInvoice: 'ADV',
            paidAmountVnd: 0,
            paidAmountUsd: 0
        });
        this._store.dispatch(InsertAdvance({ data: newInvoiceWithAdv }));

        ///this.invoices.push(newInvoiceWithAdv);
    }

    getCurrencyInvoice(invoiceList: ReceiptInvoiceModel[]): string[] {
        if (!invoiceList.length) { return ["VND"] };
        const currencyList = [...new Set(invoiceList.map(i => i.currencyId))];

        return currencyList;
    }

    processClear() {
        this.isSubmitted = true;
        if (this.form.invalid) {
            return;
        }
        const body: IProcessClearInvoiceModel = {
            currency: this.currencyId.value,
            finalExchangeRate: this.exchangeRate.value,
            paidAmount: this.finalPaidAmount.value,
            list: this.invoices.filter(x => x.type !== 'ADV'),
            customerId: this.customerInfo?.id
        };
        if (!body.customerId || !body.list.length || !body.paidAmount) {
            this._toastService.warning('Missing data to process', 'Warning');
            return;
        }
        if (this.getCurrencyInvoice(body.list).length === 2) {
            this._toastService.warning('List invoice should only have one currency', 'Warning');
            return;
        }
        this._accountingRepo.processInvoiceReceipt(body)
            .subscribe(
                (data: IProcessResultModel) => {
                    if (data?.invoices?.length) {
                        this.invoices.length = 0;
                        this.invoices = [...data.invoices];

                        this.balance.setValue(data.balance);
                    } else {
                        console.log(data);
                    }
                }
            );


    }

    getFinalPaidAmount() {
        const exChangeRateUSD = this.currencyId.value !== 'USD' ? this.exchangeRateUsd : 1;
        const exChangeRateVND = this.currencyId.value === 'USD' ? this.exchangeRateUsd : 1;
        this.finalPaidAmountUSD.setValue(this.formatNumberCurrency(((this.cusAdvanceAmount.value / exChangeRateUSD) + this.amountUSD.value + this.paidAmountUSD.value), 2));
        this.finalPaidAmountVND.setValue(this.formatNumberCurrency(Math.round((this.cusAdvanceAmount.value * exChangeRateVND) + this.amountVND.value + this.paidAmountVND.value), 0));
    }

    caculatorAmountFromDebitList(){
        let valueUSD = 0;
        let valueVND = 0;
        const exChangeRateUSD = this.currencyId.value !== 'USD' ? this.exchangeRateUsd : 1;
        const exChangeRateVND = this.currencyId.value === 'USD' ? this.exchangeRateUsd : 1;
        this.creditList
        .subscribe((x: ReceiptInvoiceModel[]) => {
            valueUSD += x.reduce((amount: number, item: ReceiptInvoiceModel) => amount += item.unpaidAmountUsd, 0);
            valueVND += x.reduce((amount: number, item: ReceiptInvoiceModel) => amount += item.unpaidAmountVnd, 0);
        });
        this.amountVND.setValue(valueUSD);
        this.amountVND.setValue(valueVND);
        this.finalPaidAmountUSD.setValue(this.formatNumberCurrency((this.cusAdvanceAmount.value / exChangeRateUSD) + valueUSD + this.paidAmountUSD.value, 2));
        this.finalPaidAmountVND.setValue(this.formatNumberCurrency(Math.round((this.cusAdvanceAmount.value * exChangeRateVND) + valueVND + this.paidAmountVND.value), 0));
    }
}

interface IProcessClearInvoiceModel {
    paidAmount: number;
    currency: string;
    list: ReceiptInvoiceModel[];
    finalExchangeRate: number;
    customerId: string;
}

interface IProcessResultModel {
    balance: number;
    invoices: ReceiptInvoiceModel[];
}
