import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ReceiptInvoiceModel, Currency, Partner } from '@models';
import { NgProgress } from '@ngx-progressbar/core';
import { AccountingRepo, CatalogueRepo } from '@repositories';
import { SortService, DataService } from '@services';
import { formatDate } from '@angular/common';
import { AppList } from '@app';
import { FormGroup, FormBuilder, AbstractControl } from '@angular/forms';
import { IAppState, getCatalogueCurrencyState, GetCatalogueCurrencyAction } from '@store';

import { Store } from '@ngrx/store';
import { catchError, finalize, takeUntil, pluck } from 'rxjs/operators';
import { Observable, from } from 'rxjs';
import { customerPaymentReceipInvoiceListState, customerPaymentReceipLoadingState } from '../../store/reducers';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'customer-payment-list-receipt',
    templateUrl: './receipt-payment-list.component.html'
})

export class ARCustomerPaymentReceiptPaymentListComponent extends AppList implements OnInit {

    invoices: ReceiptInvoiceModel[] = [];

    form: FormGroup;
    paidAmount: AbstractControl;
    methods: CommonInterface.ICommonTitleValue[];
    userLogged: SystemInterface.IClaimUser;
    type: AbstractControl;
    cusAdvanceAmount: AbstractControl;
    finalPaidAmount: AbstractControl;
    balance: AbstractControl;
    paymentMethod: AbstractControl;
    currency: AbstractControl;
    paymentDate: AbstractControl;
    exchangeRate: AbstractControl;
    bankAcountNo: AbstractControl;

    $currencyList: Observable<Currency[]>;


    paymentMethods: string[] = ['Cash', 'Bank Transfer'];
    receiptTypes: string[] = ['Debit', 'NetOff Adv'];

    customerInfo: Partner;

    constructor(
        private _sortService: SortService,
        private _progressService: NgProgress,
        private _router: Router,
        private _accountingRepo: AccountingRepo,
        private _store: Store<IAppState>,
        private _fb: FormBuilder,
        private _dataService: DataService,
        private _catalogueRepo: CatalogueRepo,
        private _toastService: ToastrService
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
            { title: 'Paid Amount', field: 'paidAmount', sortable: true },
            { title: 'Balance Amount', field: 'invoiceBalance', sortable: true },
            { title: 'Reference Amount', field: 'refAmout', sortable: true },
            { title: 'Ref Curr', field: 'refCurr', sortable: true },
            { title: 'Payment Status', field: 'paymentStatus', sortable: true },
            { title: 'Billing Date', field: 'billingDate', sortable: true },
            { title: 'Invoice Date', field: 'invoiceDate', sortable: true },
            { title: 'Note', field: 'note', sortable: true },
        ];

        this.isLoading = this._store.select(customerPaymentReceipLoadingState);

        // * subscribe invoice list
        this._store.select(customerPaymentReceipInvoiceListState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (res: ReceiptInvoiceModel[]) => {
                    this.invoices = [...res];
                    this.balance.setValue(null);
                    console.log(this.invoices);
                });


        this._dataService.currentMessage
            .pipe(pluck('cus-advance'))
            .subscribe(
                (data) => {
                    if (data !== undefined) {
                        if (!this.cusAdvanceAmount.value) {
                            this.cusAdvanceAmount.setValue(data);
                        }
                    }
                }
            );
        this._dataService.currentMessage
            .pipe(pluck('customer'))
            .subscribe(
                (data: Partner) => {
                    if (data !== undefined) {
                        this.customerInfo = data;
                        console.log(this.customerInfo);
                    }
                }
            );
    }

    initForm() {
        this.form = this._fb.group({
            paidAmount: [],
            type: [[this.receiptTypes[0]]],
            cusAdvanceAmount: [],
            finalPaidAmount: [{ value: null, disabled: true }],
            balance: [{ value: null, disabled: true }],
            paymentMethod: [this.paymentMethods[0]],
            currency: ['VND'],
            paymentDate: [{ startDate: new Date(), endDate: new Date() }],
            exchangeRate: [],
            bankAcountNo: [],
        });

        this.paidAmount = this.form.controls['paidAmount'];
        this.type = this.form.controls['type'];
        this.cusAdvanceAmount = this.form.controls['cusAdvanceAmount'];
        this.finalPaidAmount = this.form.controls['finalPaidAmount'];
        this.balance = this.form.controls['balance'];
        this.paymentMethod = this.form.controls['paymentMethod'];
        this.paymentDate = this.form.controls['paymentDate'];
        this.exchangeRate = this.form.controls['exchangeRate'];
        this.bankAcountNo = this.form.controls['bankAcountNo'];
        this.currency = this.form.controls['currency'];

        // Load tỷ giá
        this.exchangeRate.setValue(1);
    }

    generateExchangeRate(date: string, fromCurrency: string) {
        this._catalogueRepo.convertExchangeRate(date, fromCurrency)
            .pipe()
            .subscribe(
                (data: { rate: number }) => {
                    this.exchangeRate.setValue(data?.rate);
                }
            );
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
                if ((data as Currency).id === 'VND') {
                    this.exchangeRate.setValue(1);
                    break;
                }
                if (this.paymentDate.value?.startDate) {
                    this.generateExchangeRate(formatDate(this.paymentDate.value?.startDate, 'yyy-MM-dd', 'en'), (data as Currency).id);
                }

                this.balance.setValue(null);
                this.finalPaidAmount.setValue(null);
                this.paidAmount.setValue(null);
                break;

            case 'payment-date':
                if (this.currency.value === 'VND') {
                    this.exchangeRate.setValue(1);
                    break;
                }
                this.generateExchangeRate(formatDate(data?.startDate, 'yyy-MM-dd', 'en'), this.currency.value);
                break;
            default:
                break;
        }
    }

    removeInvoiceItem() {
        if (!!this.invoices.length) {
            this.invoices = this.invoices.filter((item: any) => !item.isSelected);
            return;
        }
        this.isCheckAll = false;
    }

    insertAdvanceRowData() {
        const newInvoiceWithAdv: ReceiptInvoiceModel = new ReceiptInvoiceModel({
            type: 'ADV',
            paidAmount: 0,
            unpaidAmount: 0,
            invoiceBalance: 0,
            taxCode: this.customerInfo?.taxCode,
            partnerName: this.customerInfo?.shortName
        });
        this.invoices.push(newInvoiceWithAdv);
    }

    processClear() {
        const body: IProcessClearInvoiceModel = {
            currency: this.currency.value,
            finalExchangeRate: this.exchangeRate.value,
            paidAmount: this.finalPaidAmount.value,
            list: this.invoices,
            customerId: this.customerInfo?.id
        };
        if (!body.customerId || !body.list.length) {
            this._toastService.warning('Missing data to process', 'Warning');
            return;
        }
        this._accountingRepo.processInvoiceReceipt(body)
            .subscribe(
                (data: IProcessResultModel) => {
                    if (data?.invoices?.length) {
                        this.invoices.length = 0;
                        this.invoices = data.invoices;

                        this.balance.setValue(data.balance);
                    } else {
                        console.log(data);
                    }
                }
            );


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
