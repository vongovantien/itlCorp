import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AppForm } from '@app';
import { Bank, Currency, Partner, User } from '@models';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { finalize } from 'rxjs/operators';

import { coerceBooleanProperty } from '@angular/cdk/coercion';
import { CommonEnum } from '@enums';
import { Store } from '@ngrx/store';
import { GetCatalogueBankAction, getCatalogueBankState, GetCatalogueCurrencyAction, getCatalogueCurrencyState, getCurrentUserState, IAppState } from '@store';
import { Observable } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';

@Component({
    selector: 'adv-payment-form-create',
    templateUrl: './form-create-advance-payment.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})

export class AdvancePaymentFormCreateComponent extends AppForm {
    @Input() mode: string = 'create';
    @Output() onChangeCurrency: EventEmitter<any> = new EventEmitter<any>();
    @Output() onChangeAdvanceFor: EventEmitter<string> = new EventEmitter<string>();
    @Input() set readOnly(val: any) {
        this._readonly = coerceBooleanProperty(val);
    }

    get readonlyForm(): boolean {
        return this._readonly;
    }

    private _readonly: boolean = false;
    methods: CommonInterface.ICommonTitleValue[] = [
        { title: 'Cash', value: 'Cash' },
        { title: 'Bank Transfer', value: 'Bank' },
        { title: 'Other', value: 'Other' },
    ];
    currencyList: Currency[] = [];
    userLogged: Partial<SystemInterface.IClaimUser>;
    advanceForDatas: string[] = ["HBL"]; // update MBL sau , "MBL"

    users: Observable<User[]>;
    customers: Observable<Partner[]>;

    formCreate: FormGroup;
    advanceNo: AbstractControl;
    requester: AbstractControl;
    requestDate: AbstractControl;
    paymentMethod: AbstractControl;
    statusApproval: AbstractControl;
    deadlinePayment: AbstractControl;
    note: AbstractControl;
    currency: AbstractControl;
    bankAccountName: AbstractControl;
    bankAccountNo: AbstractControl;
    bankName: AbstractControl;
    paymentTerm: AbstractControl;
    payee: AbstractControl;
    advanceFor: AbstractControl;
    dueDate: AbstractControl;

    selectedPayee: Partner;
    banks: Observable<Bank[]>;
    bankAccount: Bank[] = [];
    bankCode: AbstractControl;
    displayFieldBank: CommonInterface.IComboGridDisplayField[] = [
        { field: 'code', label: 'Bank Code' },
        { field: 'bankNameEn', label: 'Bank Name EN' },
    ];

    displayFieldBankAccount: CommonInterface.IComboGridDisplayField[] = [
        { field: 'bankAccountNo', label: 'Bank Account No' },
        { field: 'bankAccountName', label: 'Bank Account Name' },
    ];

    isAdvCarrier: boolean = false;
    payeeName: string = '';

    constructor(
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo,
        private _store: Store<IAppState>
    ) {
        super();

    }

    ngOnInit() {
        this._store.dispatch(new GetCatalogueCurrencyAction());
        this._store.dispatch(new GetCatalogueBankAction());

        this.initForm();
        this.getCurrency();

        this.banks = this._store.select(getCatalogueBankState);
        this.users = this._systemRepo.getListSystemUser();
        this.customers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL);
        this._store.select(getCurrentUserState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((u) => {
                if (!!u) {
                    this.userLogged = u;
                    this.requester.setValue(u.id);
                }
            })
    }

    initForm() {
        this.formCreate = this._fb.group({
            advanceNo: [{ value: null, disabled: true }],
            requester: [{ value: null, disabled: true }],
            statusApproval: ['New'],
            requestDate: [{
                startDate: new Date(),
                endDate: new Date(),
            }],
            deadlinePayment: [
                {
                    startDate: new Date(new Date().setDate(new Date().getDate() + 9)),
                    endDate: new Date(new Date().setDate(new Date().getDate() + 9)),
                }
            ],
            paymentMethod: [this.methods[0].value],
            note: [],
            currency: [],
            paymentTerm: [9],
            bankAccountNo: [],
            bankAccountName: [],
            bankName: [],
            payee: this.isAdvCarrier ? [null, Validators.required] : [],
            bankCode: [{ value: null, disabled: true }],
            advanceFor: [this.advanceForDatas[0]],
            dueDate: [null, Validators.required]
        });

        this.advanceNo = this.formCreate.controls['advanceNo'];
        this.requester = this.formCreate.controls['requester'];
        this.requestDate = this.formCreate.controls['requestDate'];
        this.deadlinePayment = this.formCreate.controls['deadlinePayment'];
        this.currency = this.formCreate.controls['currency'];
        this.note = this.formCreate.controls['note'];
        this.statusApproval = this.formCreate.controls['statusApproval'];
        this.paymentMethod = this.formCreate.controls['paymentMethod'];
        this.bankAccountName = this.formCreate.controls['bankAccountName'];
        this.bankAccountNo = this.formCreate.controls['bankAccountNo'];
        this.bankName = this.formCreate.controls['bankName'];
        this.paymentTerm = this.formCreate.controls['paymentTerm'];
        this.payee = this.formCreate.controls['payee'];
        this.bankCode = this.formCreate.controls['bankCode'];
        this.advanceFor = this.formCreate.controls['advanceFor'];
        this.dueDate = this.formCreate.controls['dueDate'];

        // * Detect form value change.
        this.paymentTerm.valueChanges.pipe(
            distinctUntilChanged(),
            debounceTime(250),
            takeUntil(this.ngUnsubscribe)
        ).subscribe(
            (value: number) => {
                if (this.mode !== 'approve') {
                    if (value <= 999) {
                        const deadline: CommonInterface.IMoment = {
                            startDate: new Date(new Date().setDate(new Date().getDate() + value)),
                            endDate: new Date(new Date().setDate(new Date().getDate() + value)),
                        };
                        this.deadlinePayment.setValue(deadline);
                    }
                }
            }
        );
    }

    onUpdateRequestDate(value: { startDate: any; endDate: any }) {
        this.minDate = value.startDate;
        this.deadlinePayment.setValue({
            startDate: new Date(new Date(value.startDate).setDate(new Date(value.startDate).getDate() + this.paymentTerm.value)),
            endDate: new Date(new Date(value.endDate).setDate(new Date(value.endDate).getDate() + this.paymentTerm.value)),
        });
    }

    getCurrency() {
        this._store.select(getCatalogueCurrencyState)
            .pipe(catchError(this.catchError))
            .subscribe(
                (data: any) => {
                    this.currencyList = data || [];
                    this.currency.setValue("VND");
                },
            );
    }

    changeCurrency(currency: string) {
        if (!!currency) {
            this.onChangeCurrency.emit(currency);
        }
    }

    changeAdvanceFor(data: any) {
        if (!!data) {
            console.log('changeAdvanceFor', data)
            this.onChangeAdvanceFor.emit(data);
        }
    }

    onChangePaymentMethod(method: string) {
        if (method !== 'Cash') {
            if (!this.payee.value) {
                this.bankAccountName.setValue(this.userLogged.nameVn || null);
                this.bankAccountNo.setValue(this.userLogged.bankAccountNo || null);
                this.bankName.setValue(this.userLogged.bankName || null);
                this.bankCode.setValue(this.userLogged.bankCode || null);
            } else {
                if (!this.selectedPayee) {
                    this.getPartnerById(this.payee.value)
                }
                this.getBankAccountPayee(true);
            }
        }
        else {
            this.bankAccountName.setValue(null);
            this.bankAccountNo.setValue(null);
            this.bankName.setValue(null);
            this.bankCode.setValue(null);
        }
    }

    getPartnerById(id: string) {
        this.customers.pipe(
            takeUntil(this.ngUnsubscribe)
        ).subscribe(x => this.selectedPayee = x.find(s => s.id = id));
    }

    onSelectPayee(payee: Partner) {
        this.selectedPayee = payee;
        if (this.paymentMethod.value !== 'Cash') {
            this.getBankAccountPayee(true);
        }
    }

    getBankAccountPayee(isSetBank: Boolean) {
        if (!!this.payee.value && this.paymentMethod.value !== 'Cash') {
            this._catalogueRepo.getListBankByPartnerById(this.payee.value)
                .pipe(catchError(this.catchError), finalize(() => {
                    this.isLoading = false;
                })).subscribe(
                    (res: any[]) => {
                        if (!!res && res.length > 0) {
                            this.bankAccount = res;
                            if (isSetBank === true) {
                                this.bankAccountNo.setValue(res[0].bankAccountNo);
                                this.bankAccountName.setValue(res[0].bankAccountName);
                                this.bankName.setValue(res[0].bankNameEn);
                                this.mapBankCode(res[0].code);
                            }
                        }
                        else {
                            this.bankAccount = [];
                            this.setBankInfoForPayee(this.selectedPayee);
                        }
                    })
        }
    }

    setBankInfoForPayee(payee: Partner) {
        this.bankAccountNo.setValue(payee.bankAccountNo);
        this.bankAccountName.setValue(payee.bankAccountName);
        this.bankName.setValue(payee.bankName);
        this.mapBankCode(payee.bankCode);
    }

    onSelectDataBankInfo(data: any) {
        if (data) {
            this.bankName.setValue(data.bankNameEn);
            // this.bankAccountName.setValue(data.bankAccountName)
            // this.bankAccountNo.setValue(data.bankAccountNo)
            this.mapBankCode(data.code);
        }
    }

    mapBankCode(data: any) {
        this.bankCode.setValue(data);
    }
}
