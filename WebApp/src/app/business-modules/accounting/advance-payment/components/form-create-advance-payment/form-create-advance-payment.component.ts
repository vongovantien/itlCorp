import { Component } from '@angular/core';
import { User, Currency } from 'src/app/shared/models';
import { BaseService } from 'src/app/shared/services';
import { SystemRepo } from 'src/app/shared/repositories';
import { catchError } from 'rxjs/operators';
import { AppForm } from 'src/app/app.form';
import { FormBuilder, FormGroup, AbstractControl } from '@angular/forms';

@Component({
    selector: 'adv-payment-form-create',
    templateUrl: './form-create-advance-payment.component.html'
})

export class AdvancePaymentFormCreateComponent extends AppForm {

    methods: CommonInterface.ICommonTitleValue[];
    currencyList: Currency[];
    userLogged: User;

    formCreate: FormGroup;
    advanceNo: AbstractControl;
    requester: AbstractControl;
    requestDate: AbstractControl;
    paymentMethod: AbstractControl;
    department: AbstractControl;
    deadLine: AbstractControl;
    note: AbstractControl;
    currency: AbstractControl;


    constructor(
        private _fb: FormBuilder,
        private _baseService: BaseService,
        private _sysRepo: SystemRepo
    ) {
        super();

    }

    ngOnInit() {
        this.initForm();
        this.initBasicData();
        this.getUserLogged();
        this.getCurrency();
    }

    initForm() {
        this.formCreate = this._fb.group({
            advanceNo: [],
            requester: [],
            department: [],
            requestDate: [new Date()],
            deadLine: [{
                startDate: new Date(new Date().setDate(new Date().getDate() + 7)),
                endDate: new Date(new Date().setDate(new Date().getDate() + 7)),
            }],
            paymentMethod: [],
            note: [],
            currency: []
        });

        this.advanceNo = this.formCreate.controls['advanceNo'];
        this.requester = this.formCreate.controls['requester'];
        this.requestDate = this.formCreate.controls['requestDate'];
        this.deadLine = this.formCreate.controls['deadLine'];
        this.currency = this.formCreate.controls['currency'];
        this.note = this.formCreate.controls['note'];
        this.department = this.formCreate.controls['department'];
        this.paymentMethod = this.formCreate.controls['paymentMethod'];
    }

    initBasicData() {
        this.methods = this.getMethod();
        this.paymentMethod.setValue(this.methods[0]);

    }

    getMethod(): CommonInterface.ICommonTitleValue[] {
        return [
            { title: 'Cash', value: 'Cash' },
            { title: 'Bank Transer', value: 'Bank' },
        ];
    }

    getUserLogged() {
        this.userLogged = this._baseService.getUserLogin() || 'admin';
        this.requester.setValue(this.userLogged.id);
    }

    getCurrency() {
        this._sysRepo.getListCurrency()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.currencyList = res || [];
                    this.currency.setValue(this.currencyList.filter((item: Currency) => item.id === 'VND')[0]);
                },
                (errors: any) => { },
                () => { }
            );
    }





}
