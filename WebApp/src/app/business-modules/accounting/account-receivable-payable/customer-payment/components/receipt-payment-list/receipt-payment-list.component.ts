import { Component, OnInit, Input, TemplateRef, ChangeDetectionStrategy, ViewChild } from '@angular/core';
import { ReceiptInvoiceModel, Currency, Partner } from '@models';
import { AccountingRepo, CatalogueRepo, SystemRepo } from '@repositories';
import { formatDate, formatCurrency } from '@angular/common';
import { FormGroup, FormBuilder, AbstractControl, Validators } from '@angular/forms';
import { IAppState, getCatalogueCurrencyState, GetCatalogueCurrencyAction, getCurrentUserState } from '@store';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';
import { AppForm } from '@app';
import { JobConstants, AccountingConstants } from '@constants';

import {
    ReceiptCreditListState,
    ReceiptDebitListState,
    ReceiptPartnerCurrentState,
    ReceiptAgreementCreditCurrencyState,
    ReceiptClassState
} from '../../store/reducers';
import {
    InsertAdvance,
    ProcessClearInvoiceModel,
    ProcessClearSuccess,
    ToggleAutoConvertPaid,
    SelectReceiptCurrency,
    UpdateReceiptExchangeRate
} from '../../store/actions';
import { ARCustomerPaymentReceiptDebitListComponent } from '../receipt-debit-list/receipt-debit-list.component';
import { ARCustomerPaymentReceiptCreditListComponent } from '../receipt-credit-list/receipt-credit-list.component';

import { takeUntil } from 'rxjs/operators';
import { Observable } from 'rxjs';
import cloneDeep from 'lodash/cloneDeep';
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

    form: FormGroup;
    cusAdvanceAmountVnd: AbstractControl;
    paymentMethod: AbstractControl;
    currencyId: AbstractControl;
    paymentDate: AbstractControl;
    exchangeRate: AbstractControl;
    bankAccountNo: AbstractControl;
    description: AbstractControl;
    creditAmountVnd: AbstractControl;
    creditAmountUsd: AbstractControl;
    paidAmountVnd: AbstractControl;
    paidAmountUsd: AbstractControl;
    finalPaidAmountVnd: AbstractControl;
    finalPaidAmountUsd: AbstractControl;
    cusAdvanceAmountUsd: AbstractControl;

    isAutoConvert: AbstractControl;
    isAsPaidAmount: AbstractControl;
    obhpartnerId: AbstractControl;
    notifyDepartment: AbstractControl;

    userLogged$: Observable<Partial<SystemInterface.IClaimUser>>;
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
    exchangeRateValue: number = 1;

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
        this.userLogged$ = this._store.select(getCurrentUserState);

        this.initForm();
        this.listenCustomerInfoData();
        this.listenAgreementData();

        this.generateExchangeRate(formatDate(this.paymentDate.value?.startDate, 'yyyy-MM-dd', 'en'))
            .then(
                (exchangeRate: IExchangeRate) => {
                    if (!!exchangeRate) {
                        this.exchangeRateValue = exchangeRate.rate;
                    } else {
                        this.exchangeRateValue = 0;
                    }

                    this._store.dispatch(UpdateReceiptExchangeRate({ exchangeRate: this.exchangeRateValue }));
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
            paidAmountVnd: [0, Validators.required],
            paidAmountUsd: [{ value: 0, disabled: true }, Validators.required],
            cusAdvanceAmountVnd: [],
            finalPaidAmount: [{ value: 0, disabled: true }],
            paymentMethod: [this.paymentMethods[0]],
            currencyId: ['VND'],
            paymentDate: [{ startDate: new Date(), endDate: new Date() }],
            exchangeRate: [1, Validators.required],
            bankAccountNo: [],
            creditAmountVnd: [0],
            creditAmountUsd: [{ value: 0, disabled: true }],
            description: [],
            finalPaidAmountVnd: [0],
            finalPaidAmountUsd: [0],
            isAutoConvert: [true],
            isAsPaidAmount: [false],
            obhpartnerId: [],
            notifyDepartment: [],
            cusAdvanceAmountUsd: [{ value: 0, disabled: true }]

        });

        this.cusAdvanceAmountVnd = this.form.controls['cusAdvanceAmountVnd'];
        this.paymentMethod = this.form.controls['paymentMethod'];
        this.paymentDate = this.form.controls['paymentDate'];
        this.exchangeRate = this.form.controls['exchangeRate'];
        this.bankAccountNo = this.form.controls['bankAccountNo'];
        this.currencyId = this.form.controls['currencyId'];
        this.description = this.form.controls['description'];
        this.creditAmountVnd = this.form.controls['creditAmountVnd'];
        this.creditAmountUsd = this.form.controls['creditAmountUsd'];
        this.paidAmountVnd = this.form.controls['paidAmountVnd'];
        this.paidAmountUsd = this.form.controls['paidAmountUsd'];
        this.finalPaidAmountVnd = this.form.controls['finalPaidAmountVnd'];
        this.finalPaidAmountUsd = this.form.controls['finalPaidAmountUsd'];
        this.isAutoConvert = this.form.controls['isAutoConvert'];
        this.isAsPaidAmount = this.form.controls['isAsPaidAmount'];
        this.obhpartnerId = this.form.controls['obhpartnerId'];
        this.notifyDepartment = this.form.controls['notifyDepartment'];
        this.cusAdvanceAmountUsd = this.form.controls['cusAdvanceAmountUsd'];
    }

    async generateExchangeRate(date: string, curreny: string = 'USD') {
        try {
            const exchangeRate: IExchangeRate = await this._catalogueRepo.convertExchangeRate(date, curreny).toPromise();
            if (!!exchangeRate) {
                return exchangeRate;
            }
        } catch (error) {

        }
    }

    getBankAccountNo(event: any) {
        if (event === this.paymentMethods[1]) {
            this.userLogged$
                .pipe(takeUntil(this.ngUnsubscribe))
                .subscribe((u) => {
                    if (!!u) {
                        if (this.currencyId.value === 'VND') {
                            this.bankAccountNo.setValue(u.bankOfficeAccountNoVnd)
                        } else {
                            this.bankAccountNo.setValue(u.bankOfficeAccountNoUsd)
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
                if (this.isAutoConvert.value) {
                    if (this.exchangeRateValue === 0) {
                        this.paidAmountUsd.setValue(0);
                    } else {
                        this.paidAmountUsd.setValue(formatCurrency(+((this.paidAmountVnd.value / this.exchangeRateValue).toFixed(2)), 'en', ''));
                    }
                }

                this.calculateFinalPaidAmount();
                break;
            case 'paid-amountUsd':
                if (this.isAutoConvert.value) {
                    this.paidAmountVnd.setValue(+(this.paidAmountUsd.value * this.exchangeRateValue).toFixed(0));
                }

                this.calculateFinalPaidAmount();
                break;
            case 'currency':
                if ((data as Currency).id === 'VND') {
                    this.exchangeRate.setValue(1);
                    this.creditAmountVnd.enable();
                    this.cusAdvanceAmountVnd.enable();
                    this.paidAmountVnd.enable();
                    this.creditAmountVnd.enable();

                    if (this.isAutoConvert) {
                        this.creditAmountUsd.disable();
                        this.cusAdvanceAmountUsd.disable();
                        this.paidAmountUsd.disable();
                        this.creditAmountUsd.disable();
                    }
                } else {
                    this.exchangeRate.setValue(this.exchangeRateValue);
                    this.creditAmountUsd.enable();
                    this.cusAdvanceAmountUsd.enable();
                    this.paidAmountUsd.enable();
                    this.creditAmountUsd.enable();

                    if (this.isAutoConvert) {
                        this.creditAmountVnd.disable();
                        this.cusAdvanceAmountVnd.disable();
                        this.paidAmountVnd.disable();
                        this.creditAmountVnd.disable();
                    }

                }
                this._store.dispatch(SelectReceiptCurrency({ currency: data.id }));
                this.calculateFinalPaidAmount();
                break;
            case 'creditAmountVnd':
                if (this.isAutoConvert.value) {
                    if (this.exchangeRateValue === 0) {
                        this.creditAmountUsd.setValue(0);
                    } else {
                        this.creditAmountUsd.setValue(formatCurrency(+((this.creditAmountVnd.value / this.exchangeRateValue).toFixed(2)), 'en', ''));
                    }
                }
                this.calculateFinalPaidAmount();
                break;
            case 'creditAmountUsd':
                if (this.isAutoConvert.value) {
                    this.creditAmountVnd.setValue(+(this.creditAmountUsd.value * this.exchangeRateValue).toFixed(0));
                }
                this.calculateFinalPaidAmount();
                break;

            case 'exchangeRate':
                if (!data.target.value.length) {
                    this.exchangeRate.setValue(0);
                }
                break;
            case 'cusAdvanceAmountVnd':
                if (!data.target.value.length) {
                    this.cusAdvanceAmountVnd.setValue(0);
                }
                if (!!this.isAutoConvert.value) {
                    const valueUsd: number = +((this.cusAdvanceAmountVnd.value ?? 0) / this.exchangeRateValue).toFixed(2);
                    this.cusAdvanceAmountUsd.setValue(formatCurrency(valueUsd, 'en', ''));
                }
                this.calculateFinalPaidAmount();
                break;
            case 'cusAdvanceAmountUsd':
                if (!data.target.value.length) {
                    this.cusAdvanceAmountUsd.setValue(0);
                }
                if (!!this.isAutoConvert.value) {
                    const valueVnd: number = +((this.cusAdvanceAmountUsd.value ?? 0) * this.exchangeRateValue).toFixed(0);
                    this.cusAdvanceAmountVnd.setValue(valueVnd);
                }
                this.calculateFinalPaidAmount();

                break;
            case 'payment-date':
                this.generateExchangeRate(formatDate(data?.startDate, 'yyy-MM-dd', 'en'), this.currencyId.value).then(
                    (exchangeRate: IExchangeRate) => {
                        if (!!exchangeRate) {
                            this.exchangeRateValue = exchangeRate.rate;
                        } else {
                            this.exchangeRateValue = 0;
                        }
                        this._store.dispatch(UpdateReceiptExchangeRate({ exchangeRate: this.exchangeRateValue }));

                        if (exchangeRate.rate !== this.exchangeRate.value) {
                            if (this.currencyId.value === 'VND') {
                                this.exchangeRate.setValue(1);
                            } else {
                                this.exchangeRate.setValue(this.exchangeRateValue);
                            }
                            this.calculateFinalPaidAmount();
                        }
                    }
                );

                break;
            default:
                break;
        }
    }

    private mappingReceiptTypeToAdvanceType(key: string): string {
        let otherType = '';
        switch (key) {
            case AccountingConstants.RECEIPT_CLASS.ADVANCE:
                otherType = AccountingConstants.RECEIPT_ADVANCE_TYPE.ADVANCE;
                break;
            case AccountingConstants.RECEIPT_CLASS.COLLECT_OBH:
                otherType = AccountingConstants.RECEIPT_ADVANCE_TYPE.COLLECT_OBH;
                break;
            case AccountingConstants.RECEIPT_CLASS.PAY_OBH:
                otherType = AccountingConstants.RECEIPT_ADVANCE_TYPE.PAY_OBH;
                break;
            case AccountingConstants.RECEIPT_CLASS.ADVANCE:
                otherType = AccountingConstants.RECEIPT_ADVANCE_TYPE.ADVANCE;
                break;
            default:
                break;
        }

        return otherType;
    }

    insertOtherRowData(type?: string) {
        const newInvoiceWithAdv: any = new ReceiptInvoiceModel();
        newInvoiceWithAdv.paymentType = AccountingConstants.RECEIPT_PAYMENT_TYPE.OTHER;
        if (!!type) {
            newInvoiceWithAdv.type = type.toUpperCase();
        } else {
            this.class$
                .pipe(takeUntil(this.ngUnsubscribe))
                .subscribe(
                    (receiptType) => {
                        newInvoiceWithAdv.type = this.mappingReceiptTypeToAdvanceType(receiptType);
                    })

        }
        newInvoiceWithAdv.paidAmountVnd = this.paidAmountVnd.value ?? 0;
        newInvoiceWithAdv.paidAmountUsd = this.paidAmountUsd.value ?? 0;

        newInvoiceWithAdv.totalPaidUsd = 0;
        newInvoiceWithAdv.totalPaidVnd = 0;
        newInvoiceWithAdv.unpaidAmountUsd = 0;
        newInvoiceWithAdv.unpaidAmountVnd = 0;
        newInvoiceWithAdv.refNo = null;
        newInvoiceWithAdv.id = this.utility.newGuid();

        this._store.dispatch(InsertAdvance({ data: cloneDeep(newInvoiceWithAdv) }));

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
            paidAmountVnd: this.finalPaidAmountVnd.value,
            paidAmountUsd: this.finalPaidAmountUsd.value,
            list: listInvoice.filter(x => x.type !== AccountingConstants.RECEIPT_ADVANCE_TYPE),
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

    onToggleAutoConvertPaid(isAuto: boolean) {
        if (!this.isAutoConvert.dirty) {
            return;
        }
        this._store.dispatch(ToggleAutoConvertPaid({ isAutoConvert: isAuto }));
        if (isAuto === true) {
            if (this.currencyId.value === 'VND') {
                this.paidAmountVnd.enable();
                this.cusAdvanceAmountVnd.enable();
                this.creditAmountVnd.enable();

                const _advanceUsd: number = +((this.cusAdvanceAmountVnd.value ?? 0) / this.exchangeRateValue).toFixed(2);
                this.cusAdvanceAmountUsd.setValue(formatCurrency(_advanceUsd, 'en', ''));

                this.cusAdvanceAmountUsd.disable();
                this.creditAmountUsd.disable();
                this.paidAmountUsd.disable();


            } else {
                this.paidAmountUsd.enable();
                this.cusAdvanceAmountUsd.enable();
                this.creditAmountUsd.enable();

                const _advanceVnd: number = +((this.cusAdvanceAmountUsd.value ?? 0) * this.exchangeRateValue).toFixed(0);
                this.cusAdvanceAmountVnd.setValue(_advanceVnd);

                this.cusAdvanceAmountVnd.disable();
                this.paidAmountVnd.disable();
                this.creditAmountVnd.disable();
            }
        } else {
            this.paidAmountVnd.enable();
            this.paidAmountUsd.enable();
            this.cusAdvanceAmountVnd.enable();
            this.cusAdvanceAmountUsd.enable();
            this.creditAmountUsd.enable();
            this.creditAmountVnd.enable();
        }

        this.paidAmountVnd.updateValueAndValidity();
        this.paidAmountUsd.updateValueAndValidity();

    }

    onToggleAsPaidAmount(isCheck: boolean) {
        if (!this.isAsPaidAmount.dirty) {
            return;
        }
        this.calculateFinalPaidAmount(isCheck);
    }

    calculateFinalPaidAmount(isAsPaidAmount: boolean = null) {
        if (isAsPaidAmount === null) {
            this.calculateFinalPaidAmountWithAsPaid(this.isAsPaidAmount.value);

        } else {
            this.calculateFinalPaidAmountWithAsPaid(isAsPaidAmount)
        }
    }

    private calculateFinalPaidAmountWithAsPaid(isAsPaid: boolean) {
        if (!!isAsPaid) {
            const totaValueVnd = (this.paidAmountVnd.value ?? 0) + (this.cusAdvanceAmountVnd.value ?? 0) + (this.creditAmountVnd.value ?? 0);
            const totaValueUsd = (this.paidAmountUsd.value ?? 0) + (this.cusAdvanceAmountUsd.value ?? 0) + (this.creditAmountUsd.value ?? 0);
            this.finalPaidAmountVnd.setValue(totaValueVnd);
            this.finalPaidAmountUsd.setValue(totaValueUsd);
            return;
        }
        this.finalPaidAmountVnd.setValue(this.paidAmountVnd.value + (this.creditAmountVnd.value ?? 0));
        this.finalPaidAmountUsd.setValue(this.paidAmountUsd.value + (this.creditAmountUsd.value ?? 0));
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
