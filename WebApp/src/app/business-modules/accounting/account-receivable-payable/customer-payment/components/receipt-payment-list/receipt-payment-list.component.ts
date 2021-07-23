import { Component, OnInit, Input, TemplateRef, ChangeDetectionStrategy, ViewChild } from '@angular/core';
import { ReceiptInvoiceModel, Currency, Partner } from '@models';
import { AccountingRepo, CatalogueRepo } from '@repositories';
import { DataService } from '@services';
import { formatDate, formatCurrency } from '@angular/common';
import { AppList } from '@app';
import { FormGroup, FormBuilder, AbstractControl, Validators } from '@angular/forms';
import { IAppState, getCatalogueCurrencyState, GetCatalogueCurrencyAction, getCurrentUserState } from '@store';

import { Store } from '@ngrx/store';
import { takeUntil, pluck } from 'rxjs/operators';
import { Observable, BehaviorSubject } from 'rxjs';
import { customerPaymentReceipLoadingState, ReceiptCreditListState, ReceiptDebitListState, ReceiptPartnerCurrentState, ReceiptAgreementCreditCurrencyState, ReceiptAgreementCusAdvanceState } from '../../store/reducers';
import { ToastrService } from 'ngx-toastr';
import { InsertAdvance, ProcessClearInvoiceModel, ProcessClearSuccess } from '../../store/actions';
import { ARCustomerPaymentReceiptDebitListComponent } from '../receipt-debit-list/receipt-debit-list.component';
import { ARCustomerPaymentReceiptCreditListComponent } from '../receipt-credit-list/receipt-credit-list.component';
import cloneDeep from 'lodash/cloneDeep';

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
    isAsPaidAmount: AbstractControl;

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
        private readonly _accountingRepo: AccountingRepo,
        private readonly _store: Store<IAppState>,
        private readonly _fb: FormBuilder,
        private readonly _dataService: DataService,
        private readonly _catalogueRepo: CatalogueRepo,
        private readonly _toastService: ToastrService
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
        this.listenAgreementData();
        this.generateExchangeRateUSD(formatDate(this.paymentDate.value?.startDate, 'yyyy-MM-dd', 'en'))
            .then(
                (exchangeRate: IExchangeRate) => {
                    if (!!exchangeRate) {
                        this.exchangeRateUsd = exchangeRate.rate;
                    } else {
                        this.exchangeRateUsd = 0;
                    }
                }
            );


    }

    formatNumberCurrency(input: number, digit: number) {
        return input.toLocaleString(
            'en-Us', // leave undefined to use the browser's locale, or use a string like 'en-US' to override it.
            { minimumFractionDigits: digit }
        );
    }

    listenCusAdvanceData() {
        this._store.select(ReceiptAgreementCusAdvanceState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (data) => {
                    data !== undefined && !this.cusAdvanceAmount.value && this.cusAdvanceAmount.setValue(data);
                }
            );
    }

    listenCustomerInfoData() {
        this._store.select(ReceiptPartnerCurrentState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (data: string) => {
                    if (!!data) {
                        this.partnerId = data;
                    }
                }
            );
    }

    listenAgreementData() {
        this._store.select(ReceiptAgreementCreditCurrencyState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (currency: string) => {
                    currency !== undefined && !this.currencyId.value && (this.currencyId.setValue(currency));
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
            isAutoConvert: [true],
            isAsPaidAmount: [false]

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
        this.isAsPaidAmount = this.form.controls['isAsPaidAmount'];
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
                    if (this.exchangeRateUsd === 0) {
                        this.paidAmountUSD.setValue(0);
                    } else {
                        this.paidAmountUSD.setValue(+((this.paidAmountVND.value / this.exchangeRateUsd).toFixed(2)));
                    }
                }
                if (!!this.isAsPaidAmount.value) {
                    this.cusAdvanceAmount.setValue(this.form.controls[`paidAmount${this.currencyId.value}`].value);
                }
                this.getFinalPaidAmount();
                break;
            case 'paid-amountUsd':
                if (!data.target.value.length) {
                    this.paidAmountUSD.setValue(0);
                }
                if (this.isAutoConvert.value) {
                    this.paidAmountVND.setValue(formatCurrency(this.paidAmountUSD.value * this.exchangeRateUsd, 'en', ',0-3'));
                }
                if (!!this.isAsPaidAmount.value) {
                    this.cusAdvanceAmount.setValue(this.form.controls[`paidAmount${this.currencyId.value}`].value);
                }
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
            // this.caculateAmountFromDebitList(); // ? 16056
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
                listInvoice = cloneDeep<ReceiptInvoiceModel[]>(x);
            });
        const body: IProcessClearInvoiceModel = {
            currency: this.currencyId.value,
            finalExchangeRate: this.exchangeRate.value,
            paidAmountVnd: this.finalPaidAmountVND.value,
            paidAmountUsd: this.finalPaidAmountUSD.value,
            list: listInvoice.filter(x => x.type !== 'ADV'),
        };
        if (!body.list.length || !body.paidAmountVnd || !body.paidAmountUsd) {
            this._toastService.warning('Missing data to process', 'Warning');
            return;
        }

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
        // const exChangeRateUSD = this.currencyId.value !== 'USD' ? this.exchangeRateUsd : 1;
        // const exChangeRateVND = this.currencyId.value === 'USD' ? this.exchangeRateUsd : 1;
        const exChangeRate = this.currencyId.value === 'VND' ? 1 : this.exchangeRateUsd;

        let _finalPaidAmountVnd: number = (this.amountVND.value ?? 0) + (this.paidAmountVND.value ?? 0);
        let _finalPaidAmountUsd: number = (this.amountUSD.value ?? 0) + (this.paidAmountUSD.value ?? 0);

        if (!!this.isAsPaidAmount.value) {

        }
        if (!this.isAsPaidAmount.value) {
            _finalPaidAmountVnd += ((this.cusAdvanceAmount.value ?? 0) * exChangeRate);
            _finalPaidAmountUsd += ((this.cusAdvanceAmount.value ?? 0) / exChangeRate)
        }
        this.finalPaidAmountVND.setValue(_finalPaidAmountVnd ?? 0);
        this.finalPaidAmountUSD.setValue(+(+(_finalPaidAmountUsd)).toFixed(2) ?? 0);
    }

    // ! Decrepracated
    caculateAmountFromDebitList() {
        let creditAmountUSD = 0;
        let creditAmountVND = 0;
        let paidUSD = 0;
        let paidVND = 0;
        const exChangeRateUSD = this.currencyId.value !== 'USD' ? this.exchangeRateUsd : 1;
        const exChangeRateVND = this.currencyId.value === 'USD' ? this.exchangeRateUsd : 1;
        const cusAdvanceAmount = !this.cusAdvanceAmount.value ? 0 : this.cusAdvanceAmount.value;

        this._store.select(ReceiptCreditListState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((x: ReceiptInvoiceModel[]) => {
                x.reduce((amount: number, item: ReceiptInvoiceModel) => creditAmountUSD += item.unpaidAmountUsd, 0);
                x.reduce((amount: number, item: ReceiptInvoiceModel) => creditAmountVND += item.unpaidAmountVnd, 0);
            });
        this._store.select(ReceiptDebitListState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((x: ReceiptInvoiceModel[]) => {
                paidUSD += x.reduce((amount: number, item: ReceiptInvoiceModel) => amount += item.paidAmountUsd, 0);
                paidVND += x.reduce((amount: number, item: ReceiptInvoiceModel) => amount += item.paidAmountVnd, 0);
            });


        if (!!this.isAsPaidAmount.value) {
            if (this.currencyId.value === 'USD') {
                this.paidAmountUSD.setValue(cusAdvanceAmount);
            } else {
                this.paidAmountVND.setValue(cusAdvanceAmount);
            }
        } else {
            this.paidAmountUSD.setValue(+(+(paidUSD)).toFixed(2) ?? 0);
            this.paidAmountVND.setValue(paidVND);
        }

        this.amountUSD.setValue(creditAmountUSD);
        this.amountVND.setValue(creditAmountVND);

        this.finalPaidAmountUSD.setValue((exChangeRateUSD === 0 ? 0 : +((((cusAdvanceAmount / exChangeRateUSD) * 100) / 100).toFixed(2)) + creditAmountUSD + paidUSD));
        this.finalPaidAmountVND.setValue((cusAdvanceAmount * exChangeRateVND) + creditAmountVND + paidVND);
    }
}

interface IProcessClearInvoiceModel {
    currency: string;
    paidAmountVnd: number;
    paidAmountUsd: number;
    list: ReceiptInvoiceModel[];
    finalExchangeRate: number;
}

interface IExchangeRate {
    id: number;
    currencyFromID: string;
    rate: number;
    currencyToID: string;
}
