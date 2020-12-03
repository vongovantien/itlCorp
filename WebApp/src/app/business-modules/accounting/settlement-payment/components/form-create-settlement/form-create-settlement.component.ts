import { Component, Output, EventEmitter } from '@angular/core';
import { AbstractControl, FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';

import { AppForm } from '@app';
import { User, Currency, Customer } from '@models';
import { IAppState, getCatalogueCurrencyState, GetCatalogueCurrencyAction } from '@store';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { SystemConstants } from '@constants';
import { CommonEnum } from '@enums';

import { catchError, map } from 'rxjs/operators';
import { Observable } from 'rxjs';
@Component({
    selector: 'settle-payment-form-create',
    templateUrl: './form-create-settlement.component.html'
})

export class SettlementFormCreateComponent extends AppForm {

    @Output() onChangeCurrency: EventEmitter<Currency> = new EventEmitter<Currency>();

    users: Observable<User[]>;
    userLogged: User;

    form: FormGroup;
    settlementNo: AbstractControl;
    requester: AbstractControl;
    requestDate: AbstractControl;
    paymentMethod: AbstractControl;
    amount: AbstractControl;
    currency: AbstractControl;
    note: AbstractControl;
    statusApproval: AbstractControl;
    payee: AbstractControl;

    currencyList: Currency[];
    methods: CommonInterface.ICommonTitleValue[];

    customers: Observable<Customer[]>;

    constructor(
        private _fb: FormBuilder,
        private _store: Store<IAppState>,
        private _systemRepo: SystemRepo,
        private _catalogueRepo: CatalogueRepo
    ) {
        super();
    }

    ngOnInit() {
        this._store.dispatch(new GetCatalogueCurrencyAction());

        this.initFormSettlement();
        this.initBasicData();
        this.getUserLogged();
        this.getCurrency();
        this.getSystemUser();
        this.getCustomer();

    }

    initFormSettlement() {
        this.form = this._fb.group({
            'settlementNo': [{ value: null, disabled: true }],
            'requester': [{ value: null, disabled: true }],
            'requestDate': [{ startDate: new Date(), endDate: new Date() }, Validators.required],
            'paymentMethod': [],
            'amount': [{ value: null, disabled: true }],
            'currency': [],
            'note': [],
            'statusApproval': ['New'],
            'payee': []
        });


        this.settlementNo = this.form.controls['settlementNo'];
        this.requester = this.form.controls['requester'];
        this.requestDate = this.form.controls['requestDate'];
        this.paymentMethod = this.form.controls['paymentMethod'];
        this.amount = this.form.controls['amount'];
        this.currency = this.form.controls['currency'];
        this.note = this.form.controls['note'];
        this.statusApproval = this.form.controls['statusApproval'];
        this.payee = this.form.controls['payee'];

        this.currency.valueChanges.pipe(
            map((data: any) => data)
        ).subscribe((value: Currency) => {
            if (!!value) {
                this.onChangeCurrency.emit(value);
            }
        });
    }

    getUserLogged() {
        this.userLogged = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));
        this.requester.setValue(this.userLogged.id);
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

    getSystemUser() {
        this.users = this._systemRepo.getListSystemUser({ active: true });
    }

    initBasicData() {
        this.methods = this.getMethod();
        this.paymentMethod.setValue(this.methods[0]);
    }

    getMethod(): CommonInterface.ICommonTitleValue[] {
        return [
            { title: 'Cash', value: 'Cash' },
            { title: 'Bank Transfer', value: 'Bank' },
        ];
    }

    getCustomer() {
        this.customers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CUSTOMER);
    }
}
