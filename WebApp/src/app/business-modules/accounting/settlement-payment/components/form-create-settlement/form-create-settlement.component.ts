import { Component, Output, EventEmitter } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { User, Currency } from 'src/app/shared/models';
import { BaseService, DataService } from 'src/app/shared/services';
import { AbstractControl, FormGroup, FormBuilder } from '@angular/forms';
import { SystemConstants } from 'src/constants/system.const';
import { takeUntil, catchError, distinctUntilChanged, map } from 'rxjs/operators';
import { SystemRepo } from 'src/app/shared/repositories';

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
        private _baseService: BaseService,
        private _fb: FormBuilder,
        private _dataService: DataService,
        private _sysRepo: SystemRepo
    ) {
        super();
    }

    ngOnInit() {
        this.initFormSettlement();
        this.initBasicData();
        this.getUserLogged();
        this.getCurrency();

    }

    initFormSettlement() {
        this.form = this._fb.group({
            'settlementNo': [{ value: null, disabled: true }],
            'requester': [{ value: null, disabled: true }],
            'requestDate': [new Date],
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
                distinctUntilChanged((prev, curr) => prev.id === curr.id),
                map((data: any) => data)
            ).subscribe((value: Currency) => {
                this.onChangeCurrency.emit(value);
            });
    }

    getUserLogged() {
        this.userLogged = this._baseService.getUserLogin() || 'admin';
        this.requester.setValue(this.userLogged.id);
    }

    getCurrency() {
        this._dataService.getDataByKey(SystemConstants.CSTORAGE.CURRENCY)
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.currencyList = res || [];
                        this.currency.setValue(this.currencyList.filter((item: Currency) => item.id === 'VND')[0]);
                    } else {
                        this._sysRepo.getListCurrency()
                            .pipe(catchError(this.catchError))
                            .subscribe(
                                (data: any) => {
                                    this.currencyList = data || [];
                                    this.currency.setValue(this.currencyList.filter((item: Currency) => item.id === 'VND')[0]);
                                    // this._dataService.setData(SystemConstants.CSTORAGE.CURRENCY, data);

                                },
                            );
                    }
                }
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
