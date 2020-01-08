import { Component, Output, EventEmitter } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { User, Currency } from 'src/app/shared/models';
import { AbstractControl, FormGroup, FormBuilder } from '@angular/forms';
import { IAppState, getCatalogueCurrencyState, GetCatalogueCurrencyAction } from '@store';
import { Store } from '@ngrx/store';

import { catchError, map } from 'rxjs/operators';

@Component({
    selector: 'settle-payment-form-create',
    templateUrl: './form-create-settlement.component.html'
})

export class SettlementFormCreateComponent extends AppForm {

    @Output() onChangeCurrency: EventEmitter<Currency> = new EventEmitter<Currency>();

    userLogged: User;

    form: FormGroup;
    settlementNo: AbstractControl;
    requester: AbstractControl;
    requestDate: AbstractControl;
    paymentMethod: AbstractControl;
    amount: AbstractControl;
    currency: AbstractControl;
    note: AbstractControl;

    currencyList: Currency[];
    methods: CommonInterface.ICommonTitleValue[];

    constructor(
        private _fb: FormBuilder,
        private _store: Store<IAppState>
    ) {
        super();
    }

    ngOnInit() {
        this._store.dispatch(new GetCatalogueCurrencyAction());

        this.initFormSettlement();
        this.initBasicData();
        this.getUserLogged();
        this.getCurrency();

    }

    initFormSettlement() {
        this.form = this._fb.group({
            'settlementNo': [{ value: null, disabled: true }],
            'requester': [{ value: null, disabled: true }],
            'requestDate': [{ startDate: new Date(), endDate: new Date() }],
            'paymentMethod': [],
            'amount': [{ value: null, disabled: true }],
            'currency': [],
            'note': [],
        });


        this.settlementNo = this.form.controls['settlementNo'];
        this.requester = this.form.controls['requester'];
        this.requestDate = this.form.controls['requestDate'];
        this.paymentMethod = this.form.controls['paymentMethod'];
        this.amount = this.form.controls['amount'];
        this.currency = this.form.controls['currency'];
        this.note = this.form.controls['note'];

        this.currency.valueChanges.pipe(
            map((data: any) => data)
        ).subscribe((value: Currency) => {
            if (!!value) {
                this.onChangeCurrency.emit(value);
            }
        });
    }

    getUserLogged() {
        this.userLogged = JSON.parse(localStorage.getItem('id_token_claims_obj'));
        this.requester.setValue(this.userLogged.id);
    }

    getCurrency() {
        this._store.select(getCatalogueCurrencyState)
            .pipe(catchError(this.catchError))
            .subscribe(
                (data: any) => {
                    this.currencyList = data || [];
                    this.currency.setValue(this.currencyList.filter((item: Currency) => item.id === 'VND')[0]);
                },
            );
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
}
