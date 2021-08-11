import { Component, OnInit, Input, TemplateRef, ChangeDetectionStrategy, ViewChild } from '@angular/core';
import { ReceiptInvoiceModel, Currency, Partner } from '@models';
import { AccountingRepo, CatalogueRepo, SystemRepo } from '@repositories';
import { DataService } from '@services';
import { formatDate, formatCurrency } from '@angular/common';
import { FormGroup, FormBuilder, AbstractControl, Validators, AbstractControlOptions } from '@angular/forms';
import { IAppState, getCatalogueCurrencyState, GetCatalogueCurrencyAction, getCurrentUserState } from '@store';

import { Store } from '@ngrx/store';
import { takeUntil } from 'rxjs/operators';
import { Observable, BehaviorSubject } from 'rxjs';
import { ReceiptCreditListState, ReceiptDebitListState, ReceiptPartnerCurrentState, ReceiptAgreementCreditCurrencyState, ReceiptAgreementCusAdvanceState, ReceiptClassState } from '../../store/reducers';
import { ToastrService } from 'ngx-toastr';
import { InsertAdvance, ProcessClearInvoiceModel, ProcessClearSuccess, ToggleAutoConvertPaid, SelectReceiptCurrency } from '../../store/actions';
import { ARCustomerPaymentReceiptDebitListComponent } from '../receipt-debit-list/receipt-debit-list.component';
import { ARCustomerPaymentReceiptCreditListComponent } from '../receipt-credit-list/receipt-credit-list.component';
import cloneDeep from 'lodash/cloneDeep';
import { JobConstants, AccountingConstants } from '@constants';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'customer-payment-list-receipt',
    templateUrl: './receipt-payment-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})

export class ARCustomerPaymentReceiptPaymentListComponent extends AppForm implements OnInit {
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
    obhpartnerId: AbstractControl;
    notifyDepartment: AbstractControl;

    $currencyList: Observable<Currency[]>;
    paymentMethods: string[] = [
        AccountingConstants.RECEIPT_PAYMENT_METHOD.CASH,
        AccountingConstants.RECEIPT_PAYMENT_METHOD.BANK,
        AccountingConstants.RECEIPT_PAYMENT_METHOD.INTERNAL,
        AccountingConstants.RECEIPT_PAYMENT_METHOD.MANAGEMENT_FEE,
        AccountingConstants.RECEIPT_PAYMENT_METHOD.OTHER_FEE,
        AccountingConstants.RECEIPT_PAYMENT_METHOD.EXTRA,
        AccountingConstants.RECEIPT_PAYMENT_METHOD.OTHER
    ];

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
    displayFieldsPartner: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;

    obhPartners: Observable<Partner[]>;
    departments: Observable<any>;
    class$: Observable<string>;

    constructor(
        private readonly _accountingRepo: AccountingRepo,
        private readonly _store: Store<IAppState>,
        private readonly _fb: FormBuilder,
        private readonly _catalogueRepo: CatalogueRepo,
        private readonly _toastService: ToastrService,
        private readonly _systemRepo: SystemRepo
    ) {
        super();
    }

    ngOnInit() {
        this._store.dispatch(new GetCatalogueCurrencyAction());
        this.$currencyList = this._store.select(getCatalogueCurrencyState);

        this.obhPartners = this._catalogueRepo.getListPartner(null, null, { active: true, partnerMode: 'Internal' });
        this.departments = this._systemRepo.getDepartment(null, null, { active: true, deptTypes: ['AR', 'ACCOUNTANT'] });

        this.initForm();
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

        this.class$ = this._store.select(ReceiptClassState)


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
            paymentMethod: [this.paymentMethods[0]],
            currencyId: ['VND'],
            paymentDate: [{ startDate: new Date(), endDate: new Date() }],
            exchangeRate: [1, Validators.required],
            bankAccountNo: [],
            amountVND: [],
            amountUSD: [{ value: null, disabled: true } as AbstractControlOptions],
            description: [],
            finalPaidAmountVND: [],
            finalPaidAmountUSD: [],
            isAutoConvert: [true],
            isAsPaidAmount: [false],
            obhpartnerId: [],
            notifyDepartment: [],

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
        this.obhpartnerId = this.form.controls['obhpartnerId'];
        this.notifyDepartment = this.form.controls['notifyDepartment'];
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
                    this.paidAmountVND.setValue(formatCurrency(this.paidAmountUSD.value * this.exchangeRateUsd, 'en', ''));
                }
                if (!!this.isAsPaidAmount.value) {
                    this.cusAdvanceAmount.setValue(this.form.controls[`paidAmount${this.currencyId.value}`].value);
                }
                this.getFinalPaidAmount();
                break;
            case 'currency':
                if ((data as Currency).id === 'VND') {
                    this.exchangeRate.setValue(1);
                    this.amountVND.enable();
                    if (this.isAutoConvert)
                        this.amountUSD.disable();
                } else {
                    if ((data as Currency).id !== 'VND') {
                        this.exchangeRate.setValue(this.exchangeRateUsd);
                        this.amountUSD.enable();

                        if (this.isAutoConvert)
                            this.amountVND.disable();
                    } else {
                        this.exchangeRate.setValue(1);
                    }
                }
                this._store.dispatch(SelectReceiptCurrency({ currency: data.id }));
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

    insertOtherRowData(type?: string) {
        const newInvoiceWithAdv: any = new ReceiptInvoiceModel();
        newInvoiceWithAdv.paymentType = 'Other';
        if (!!type) {
            newInvoiceWithAdv.type = type;
        } else {
            this.class$
                .pipe(takeUntil(this.ngUnsubscribe))
                .subscribe(
                    (receiptType) => {
                        newInvoiceWithAdv.type = receiptType;
                    })

        }
        newInvoiceWithAdv.paidAmountVnd = this.amountVND.value ?? 0;
        newInvoiceWithAdv.paidAmountUsd = this.amountUSD.value ?? 0;

        newInvoiceWithAdv.totalPaidUsd = 0;
        newInvoiceWithAdv.totalPaidVnd = 0;
        newInvoiceWithAdv.unpaidAmountUsd = 0;
        newInvoiceWithAdv.unpaidAmountVnd = 0;
        newInvoiceWithAdv.refNo = null;

        const data = cloneDeep(newInvoiceWithAdv)
        this._store.dispatch(InsertAdvance({ data }));

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
            list: listInvoice.filter(x => x.type !== 'Other'),
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
        const exChangeRate = this.currencyId.value === 'VND' ? 1 : this.exchangeRateUsd;

        let _finalPaidAmountVnd: number = (this.amountVND.value ?? 0) + (this.paidAmountVND.value ?? 0);
        let _finalPaidAmountUsd: number = (this.amountUSD.value ?? 0) + (this.paidAmountUSD.value ?? 0);

        if (!this.isAsPaidAmount.value) {
            _finalPaidAmountVnd += ((this.cusAdvanceAmount.value ?? 0) * exChangeRate);
            _finalPaidAmountUsd += ((this.cusAdvanceAmount.value ?? 0) / exChangeRate)
        }
        this.finalPaidAmountVND.setValue(_finalPaidAmountVnd ?? 0);
        this.finalPaidAmountUSD.setValue(+(+(_finalPaidAmountUsd)).toFixed(2) ?? 0);
    }

    onToggleAutoConvertPaid(isAuto: boolean) {
        if (!this.isAutoConvert.dirty) {
            return;
        }
        this._store.dispatch(ToggleAutoConvertPaid({ isAutoConvert: isAuto }));
        if (isAuto === true) {
            if (this.currencyId.value === 'VND') {
                this.amountUSD.disable();
                this.amountVND.enable();

            } else {
                this.amountUSD.enable();
                this.amountVND.disable();
            }
        } else {
            this.amountVND.enable();
            this.amountUSD.enable();
        }

    }

    onToggleAsPaidAmount(isCheck) {
        if (!this.isAsPaidAmount.dirty) {
            return;
        }
        if (isCheck) {
            const totaValue = (this.finalPaidAmountVND.value ?? 0) + (this.cusAdvanceAmount.value ?? 0);
            this.finalPaidAmountVND.setValue(totaValue);
        } else {
            this.finalPaidAmountVND.setValue(this.finalPaidAmountVND);
        }
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
