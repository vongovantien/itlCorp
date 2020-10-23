import { Component, Output, EventEmitter, Input, ChangeDetectionStrategy } from '@angular/core';
import { User, Currency } from '@models';
import { DataService } from '@services';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { AppForm } from 'src/app/app.form';
import { FormBuilder, FormGroup, AbstractControl } from '@angular/forms';
import { SystemConstants } from '@constants';

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

    methods: CommonInterface.ICommonTitleValue[];
    currencyList: Currency[] = [];
    userLogged: SystemInterface.IClaimUser;
    users: Observable<User[]>;

    formCreate: FormGroup;
    advanceNo: AbstractControl;
    requester: AbstractControl;
    requestDate: AbstractControl;
    paymentMethod: AbstractControl;
    statusApproval: AbstractControl;
    deadLine: AbstractControl;
    note: AbstractControl;
    currency: AbstractControl;
    bankAccountName: AbstractControl;
    bankAccountNo: AbstractControl;
    bankName: AbstractControl;
    paymentTerm: AbstractControl;


    constructor(
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        private _dataService: DataService,
        private _systemRepo: SystemRepo
    ) {
        super();

    }

    ngOnInit() {
        this.initForm();
        this.initBasicData();
        this.getUserLogged();
        this.getCurrency();
    }

    ngOnChanges() {
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
            deadLine: [
                {
                    startDate: new Date(new Date().setDate(new Date().getDate() + 9)),
                    endDate: new Date(new Date().setDate(new Date().getDate() + 9)),
                }
            ],
            paymentMethod: [],
            note: [],
            currency: [],
            paymentTerm: [9],
            bankAccountNo: [],
            bankAccountName: [],
            bankName: []
        });

        this.advanceNo = this.formCreate.controls['advanceNo'];
        this.requester = this.formCreate.controls['requester'];
        this.requestDate = this.formCreate.controls['requestDate'];
        this.deadLine = this.formCreate.controls['deadLine'];
        this.currency = this.formCreate.controls['currency'];
        this.note = this.formCreate.controls['note'];
        this.statusApproval = this.formCreate.controls['statusApproval'];
        this.paymentMethod = this.formCreate.controls['paymentMethod'];
        this.bankAccountName = this.formCreate.controls['bankAccountName'];
        this.bankAccountNo = this.formCreate.controls['bankAccountNo'];
        this.bankName = this.formCreate.controls['bankName'];
        this.paymentTerm = this.formCreate.controls['paymentTerm'];

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
                        this.deadLine.setValue(deadline);
                    }
                }
            }
        );
    }

    onUpdateRequestDate(value: { startDate: any; endDate: any }) {
        this.minDate = value.startDate;
        this.deadLine.setValue({
            startDate: new Date(new Date(value.startDate).setDate(new Date(value.startDate).getDate() + this.paymentTerm.value)),
            endDate: new Date(new Date(value.endDate).setDate(new Date(value.endDate).getDate() + this.paymentTerm.value)),
        });
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

    getUserLogged() {
        this.users = this._systemRepo.getListSystemUser();
        this.userLogged = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));

        this.requester.setValue(this.userLogged.id);
    }

    getCurrency() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.CURRENCY)) {
            this.currencyList = this._dataService.getDataByKey(SystemConstants.CSTORAGE.CURRENCY) || [];
            this.currency.setValue(this.currencyList.filter((item: Currency) => item.id === 'VND')[0].id);
        } else {
            this._catalogueRepo.getListCurrency()
                .pipe(catchError(this.catchError))
                .subscribe(
                    (data: any) => {
                        this.currencyList = data || [];
                        this.currency.setValue(this.currencyList.filter((item: Currency) => item.id === 'VND')[0].id);
                    },
                );
        }
    }

    changeCurrency(currency: string) {
        if (!!currency) {
            this.onChangeCurrency.emit(currency);
        }
    }

    onChangePaymentMethod(method: CommonInterface.ICommonTitleValue) {
        if (method.value === 'Bank') {
            this.bankAccountName.setValue(this.userLogged.nameVn || null);
            this.bankAccountNo.setValue(this.userLogged.bankAccountNo || null);
            this.bankName.setValue(this.userLogged.bankName || null);
        }
    }
}
