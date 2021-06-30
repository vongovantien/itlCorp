import { Component, OnInit, Input, TemplateRef, ChangeDetectionStrategy, ViewChild } from '@angular/core';
import { ReceiptInvoiceModel, Currency, Partner } from '@models';
import { NgProgress } from '@ngx-progressbar/core';
import { AccountingRepo, CatalogueRepo } from '@repositories';
import { DataService } from '@services';
import { formatDate } from '@angular/common';
import { AppList } from '@app';
import { FormGroup, FormBuilder, AbstractControl, Validators } from '@angular/forms';
import { IAppState, getCatalogueCurrencyState, GetCatalogueCurrencyAction, getCurrentUserState } from '@store';

import { Store } from '@ngrx/store';
import { takeUntil, pluck } from 'rxjs/operators';
import { Observable, BehaviorSubject } from 'rxjs';
import { customerPaymentReceipLoadingState, ReceiptCreditListState, ReceiptDebitListState, ReceiptPartnerCurrentState } from '../../store/reducers';
import { ToastrService } from 'ngx-toastr';
import { InsertAdvance, ProcessClearInvoiceModel, ProcessClearSuccess } from '../../store/actions';
import { ARCustomerPaymentReceiptDebitListComponent } from '../receipt-debit-list/receipt-debit-list.component';
import { ARCustomerPaymentReceiptCreditListComponent } from '../receipt-credit-list/receipt-credit-list.component';
import { cloneDeep, isNumber } from 'lodash';

@Component({
    selector: 'customer-payment-list-receipt',
    templateUrl: './receipt-payment-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})

export class ARCustomerPaymentReceiptPaymentListComponent extends AppList implements OnInit {
    @ViewChild(ARCustomerPaymentReceiptDebitListComponent) receiptDebitList: ARCustomerPaymentReceiptDebitListComponent;
    @ViewChild(ARCustomerPaymentReceiptCreditListComponent) receiptCreditList: ARCustomerPaymentReceiptCreditListComponent;

    @Input() syncInfoTemplate: TemplateRef<any>

    creditList: Observable<ReceiptInvoiceModel[]> = this._store.select(ReceiptCreditListState);
    debitList: Observable<ReceiptInvoiceModel[]> = this._store.select(ReceiptDebitListState);
    term$ = new BehaviorSubject<string>('');

    form: FormGroup;
    methods: CommonInterface.ICommonTitleValue[];
    userLogged: Partial<SystemInterface.IClaimUser>;
    cusAdvanceAmount: AbstractControl;
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
    isAutoConvert: AbstractControl;

    $currencyList: Observable<Currency[]>;


    paymentMethods: string[] = ['Cash', 'Bank Transfer', 'Other'];

    partnerId: any = null;

    isSubmitted: boolean = false;
    isReadonly: boolean = null;  // * DONE | CANCEL
    exchangeRateUsd: number = 1;

    headerReceiptReadonly: CommonInterface.IHeaderTable[] = [
        { title: 'Billing Ref No', field: 'invoiceNo' },
        { title: 'Series No', field: 'serieNo' },
        { title: 'Type', field: 'type' },
        { title: 'Partner Name', field: 'partnerName' },
        { title: 'Taxcode', field: 'taxCode' },
        { title: 'Paid Amount', field: 'paidAmount' },
        { title: 'Balance Amount', field: 'invoiceBalance' },
        { title: 'Payment Status', field: 'paymentStatus' },
        { title: 'Billing Date', field: 'billingDate' },
        { title: 'Invoice Date', field: 'invoiceDate' },
        { title: 'Note', field: 'note' },
    ];

    constructor(
        private _accountingRepo: AccountingRepo,
        private _store: Store<IAppState>,
        private _fb: FormBuilder,
        private _dataService: DataService,
        private _catalogueRepo: CatalogueRepo,
    ) {
        super();
    }

    ngOnInit() {
        this._store.dispatch(new GetCatalogueCurrencyAction());
        this.$currencyList = this._store.select(getCatalogueCurrencyState);
        this.isLoading = this._store.select(customerPaymentReceipLoadingState);

        this.initForm();
        this.listenCusAdvanceData();
        this.listenCustomerInfoData();
    }

    formatNumberCurrency(input: number, digit: number) {
        return input.toLocaleString(
            'en-Us', // leave undefined to use the browser's locale, or use a string like 'en-US' to override it.
            { minimumFractionDigits: digit }
        );
    }

    listenCusAdvanceData() {
        this._dataService.currentMessage
            .pipe(pluck('cus-advance'), takeUntil(this.ngUnsubscribe))
            .subscribe(
                (data) => {
                    data !== undefined && !this.cusAdvanceAmount.value && this.cusAdvanceAmount.setValue(data);
                    if (data !== undefined) {
                        this.generateExchangeRateUSD(formatDate(this.paymentDate.value?.startDate, 'yyyy-MM-dd', 'en')).then(
                            (exchangeRate: IExchangeRate) => {
                                if (!!exchangeRate) {
                                    this.exchangeRateUsd = exchangeRate.rate;
                                    console.log(this.exchangeRateUsd);

                                } else {
                                    this.exchangeRateUsd = 0;
                                }
                                this.caculateAmountFromDebitList();
                            }
                        );
                    }
                }
            );
    }

    listenCustomerInfoData() {
        this._store.select(ReceiptPartnerCurrentState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (data: string) => {
                    data !== undefined && (this.partnerId = data);
                }
            );
    }

    listenCurrencyInfoData() {
        this._dataService.currentMessage
            .pipe(pluck('currency'), takeUntil(this.ngUnsubscribe))
            .subscribe(
                (data: Partner) => {
                    data !== undefined && !this.currencyId.value && (this.currencyId.setValue(data));
                }
            );
    }

    initForm() {
        this.form = this._fb.group({
            paidAmountVND: [null, Validators.required],
            paidAmountUSD: [null, Validators.required],
            cusAdvanceAmount: [],
            finalPaidAmount: [{ value: null, disabled: true }],
            // balance: [{ value: null, disabled: true }],
            paymentMethod: [this.paymentMethods[0]],
            currencyId: ['VND'],
            paymentDate: [{ startDate: new Date(), endDate: new Date() }],
            exchangeRate: [1, Validators.required],
            bankAccountNo: [],
            amountVND: [],
            amountUSD: [],
            description: [],
            finalPaidAmountVND: [],
            finalPaidAmountUSD: [],
            isAutoConvert: [true]
        });

        this.cusAdvanceAmount = this.form.controls['cusAdvanceAmount'];
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
        this.isAutoConvert = this.form.controls['isAutoConvert'];
    }

    async generateExchangeRateUSD(date: string) {
        try {
            const exchangeRate: IExchangeRate = await this._catalogueRepo.convertExchangeRate(date, 'USD').toPromise();
            if (!!exchangeRate) {
                return exchangeRate;
            }
        } catch (error) {

        }
    }

    getBankAccountNo(event: any) {
        if (event === this.paymentMethods[1]) {
            this._store.select(getCurrentUserState)
                .pipe(takeUntil(this.ngUnsubscribe))
                .subscribe((u) => {
                    if (!!u) {
                        this.userLogged = u;
                        if (this.currencyId.value === 'VND') {
                            this.bankAccountNo.setValue(this.userLogged.bankOfficeAccountNoVnd)
                        } else {
                            this.bankAccountNo.setValue(this.userLogged.bankOfficeAccountNoUsd)
                        }
                    }
                });

        } else {
            this.bankAccountNo.setValue(null);
        }
    }

    onSelectDataFormInfo(data, type: string) {
        switch (type) {
            case 'paid-amountVnd':
                if (!data.target.value.length) {
                    this.paidAmountVND.setValue(0);
                }
                if (this.isAutoConvert.value) {
                    console.log("Ty gia USD", this.exchangeRateUsd);
                    this.paidAmountUSD.setValue((this.exchangeRateUsd === 0 ? 0 : +(((this.paidAmountVND.value / this.exchangeRateUsd) * 100)) / 100).toFixed(3));
                }
                this.getFinalPaidAmount();
                break;
            case 'paid-amountUsd':
                if (!data.target.value.length) {
                    this.paidAmountUSD.setValue(0);
                }
                this.paidAmountVND.setValue(this.formatNumberCurrency(this.paidAmountUSD.value * this.exchangeRateUsd, 2));
                this.getFinalPaidAmount();
                break;
            case 'currency':
                if ((data as Currency).id === 'VND') {
                    this.exchangeRate.setValue(1);
                } else {
                    if ((data as Currency).id !== 'VND') {
                        this.exchangeRate.setValue(this.exchangeRateUsd);
                    } else {
                        this.exchangeRate.setValue(1);
                    }
                }
                this.getFinalPaidAmount();
                break;
            case 'amountVND':
                if (!data.target.value.length) {
                    this.amountVND.setValue(0);
                }
            case 'amountUSD':
                if (!data.target.value.length) {
                    this.amountUSD.setValue(0);
                }
            case 'exchangeRate':
                if (!data.target.value.length) {
                    this.exchangeRate.setValue(0);
                }
                break;
            case 'cusAdvanceAmount':
                if (!data.target.value.length) {
                    this.cusAdvanceAmount.setValue(0);
                }
                this.getFinalPaidAmount();
                break;
            case 'payment-date':
                this.generateExchangeRateUSD(formatDate(data?.startDate, 'yyy-MM-dd', 'en')).then(
                    (exchangeRate: IExchangeRate) => {
                        if (!!exchangeRate) {
                            this.exchangeRateUsd = exchangeRate.rate;
                        } else {
                            this.exchangeRateUsd = 0;
                        }
                        if (exchangeRate.rate !== this.exchangeRate.value) {
                            if (this.currencyId.value === 'VND') {
                                this.exchangeRate.setValue(1);
                            } else {
                                this.exchangeRate.setValue(this.exchangeRateUsd);
                            }
                            this.getFinalPaidAmount();
                        }
                    }
                );

                break;
            default:
                break;
        }
    }

    getListReceiptChange(onChange: boolean) {
        if (onChange) {
            this.caculateAmountFromDebitList();
        }
    }

    insertAdvanceRowData() {
        const newInvoiceWithAdv: any = {};
        newInvoiceWithAdv.typeInvoice = 'ADV';
        newInvoiceWithAdv.type = 'ADV';
        newInvoiceWithAdv.paidAmountVnd = 0;
        newInvoiceWithAdv.paidAmountUsd = 0;
        newInvoiceWithAdv.refNo = null;

        const data = newInvoiceWithAdv as ReceiptInvoiceModel;
        this._store.dispatch(InsertAdvance({ data: data }));

    }

    processClear() {
        this.isSubmitted = true;
        if (this.form.invalid) {
            return;
        }

        let listInvoice = [];
        this.debitList
            .subscribe((x: ReceiptInvoiceModel[]) => {
                listInvoice = cloneDeep(x);
            });
        const body: IProcessClearInvoiceModel = {
            currency: this.currencyId.value,
            finalExchangeRate: this.exchangeRate.value,
            paidAmountVnd: this.finalPaidAmountVND.value,
            paidAmountUsd: this.finalPaidAmountUSD.value,
            list: listInvoice.filter(x => x.type !== 'ADV'),
            customerId: this.partnerId
        };
        // if (!body.customerId || !body.list.length || !body.paidAmount) {
        //     this._toastService.warning('Missing data to process', 'Warning');
        //     return;
        // }
        // if (this.getCurrencyInvoice(body.list).length === 2) {
        //     this._toastService.warning('List invoice should only have one currency', 'Warning');
        //     return;
        // }
        this._accountingRepo.processInvoiceReceipt(body)
            .subscribe(
                (data: ProcessClearInvoiceModel) => {
                    if (data?.invoices?.length) {
                        this._store.dispatch(ProcessClearSuccess({ data: data }));
                    } else {
                        console.log(data);
                    }
                }
            );


    }

    getFinalPaidAmount() {
        const exChangeRateUSD = this.currencyId.value !== 'USD' ? this.exchangeRateUsd : 1;
        const exChangeRateVND = this.currencyId.value === 'USD' ? this.exchangeRateUsd : 1;

        const _finalPaidAmountVnd: number = (this.cusAdvanceAmount.value ?? 0 * exChangeRateVND) + (this.amountVND.value ?? 0) + (this.paidAmountVND.value ?? 0);
        const _finalPaidAmountUsd: number = exChangeRateUSD === 0 ? 0
            : (((this.cusAdvanceAmount.value ?? 0) / exChangeRateUSD) * 100) / 100
            + (this.amountUSD.value ?? 0) + (this.paidAmountUSD.value ?? 0);

        this.finalPaidAmountVND.setValue(_finalPaidAmountVnd ?? 0);
        this.finalPaidAmountUSD.setValue(_finalPaidAmountUsd ?? 0);
    }

    caculateAmountFromDebitList() {
        this.creditList = this._store.select(ReceiptCreditListState);
        this.debitList = this._store.select(ReceiptDebitListState);

        let valueUSD = 0;
        let valueVND = 0;
        let paidUSD = 0;
        let paidVND = 0;
        const exChangeRateUSD = this.currencyId.value !== 'USD' ? this.exchangeRateUsd : 1;
        const exChangeRateVND = this.currencyId.value === 'USD' ? this.exchangeRateUsd : 1;
        const cusAdvanceAmount = !this.cusAdvanceAmount.value ? 0 : this.cusAdvanceAmount.value;
        this.creditList
            .subscribe((x: ReceiptInvoiceModel[]) => {
                x.reduce((amount: number, item: ReceiptInvoiceModel) => valueUSD += item.unpaidAmountUsd, 0);
                x.reduce((amount: number, item: ReceiptInvoiceModel) => valueVND += item.unpaidAmountVnd, 0);
            });
        this.debitList
            .subscribe((x: ReceiptInvoiceModel[]) => {
                paidUSD += x.reduce((amount: number, item: ReceiptInvoiceModel) => amount += item.paidAmountUsd, 0);
                paidVND += x.reduce((amount: number, item: ReceiptInvoiceModel) => amount += item.paidAmountVnd, 0);
            });
        this.amountUSD.setValue(valueUSD);
        this.amountVND.setValue(valueVND);

        this.paidAmountUSD.setValue(paidUSD);
        this.paidAmountVND.setValue(paidVND);

        this.finalPaidAmountUSD.setValue((exChangeRateUSD === 0 ? 0 : +((((cusAdvanceAmount / exChangeRateUSD) * 100) / 100).toFixed(3)) + valueUSD + paidUSD));
        this.finalPaidAmountVND.setValue((cusAdvanceAmount * exChangeRateVND) + valueVND + paidVND);
    }
}

interface IProcessClearInvoiceModel {
    currency: string;
    paidAmountVnd: number;
    paidAmountUsd: number;
    list: ReceiptInvoiceModel[];
    finalExchangeRate: number;
    customerId: string;
}

interface IExchangeRate {
    id: number;
    currencyFromID: string;
    rate: number;
    currencyToID: string;
}
