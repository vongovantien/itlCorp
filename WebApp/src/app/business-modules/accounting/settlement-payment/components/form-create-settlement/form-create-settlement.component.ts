import { Component } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { User, Currency } from 'src/app/shared/models';
import { BaseService, DataService } from 'src/app/shared/services';
import { AbstractControl, FormGroup, FormBuilder } from '@angular/forms';
import { SystemConstants } from 'src/constants/system.const';
import { takeUntil, catchError } from 'rxjs/operators';
import { SystemRepo } from 'src/app/shared/repositories';

@Component({
    selector: 'settle-payment-form-create',
    templateUrl: './form-create-settlement.component.html'
})

export class SettlementFormCreateComponent extends AppForm {

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

    constructor(
        private _baseService: BaseService,
        private _fb: FormBuilder,
        private _dataService: DataService,
        private _sysRepo: SystemRepo
    ) {
        super();
    }

    ngOnInit() { 
        this.form = this._fb.group({
            'settlementNo': [],
            'requester': [],
            'requestDate': [new Date],
            'paymentMethod': [],
            'amount': [],
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

        this.getUserLogged();
        this.getCurrency();

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
                                },
                            );
                    }
                }
            );
    }
}
