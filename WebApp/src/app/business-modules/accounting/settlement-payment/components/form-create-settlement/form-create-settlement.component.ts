import { Component, Output, EventEmitter } from '@angular/core';
import { AbstractControl, FormGroup, FormBuilder, Validators } from '@angular/forms';

import { AppForm } from '@app';
import { User, Currency, Customer, Partner } from '@models';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { SystemConstants } from '@constants';
import { CommonEnum } from '@enums';

import { map } from 'rxjs/operators';
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
    beneficiaryName: AbstractControl;
    bankAccountNo: AbstractControl;
    bankName: AbstractControl;

    currencyList: any[] = [{ id: 'VND' }, { id: 'USD' }];
    methods: CommonInterface.ICommonTitleValue[];

    customers: any;

    constructor(
        private _fb: FormBuilder,
        private _systemRepo: SystemRepo,
        private _catalogueRepo: CatalogueRepo
    ) {
        super();
    }

    ngOnInit() {
        this.initFormSettlement();
        this.initBasicData();
        this.getUserLogged();
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
            'currency': ['VND'],
            'note': [],
            'statusApproval': ['New'],
            'payee': [],
            'beneficiaryName': [],
            'bankAccountNo': [],
            'bankName': []
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
        this.beneficiaryName = this.form.controls['beneficiaryName'];
        this.bankAccountNo = this.form.controls['bankAccountNo'];
        this.bankName = this.form.controls['bankName'];

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
            { title: 'Other', value: 'Other' },
        ];
    }

    getCustomer() {
        const customersFromService = this._catalogueRepo.getCurrentCustomerSource();
        if (!!customersFromService.data.length) {
            this.customers = customersFromService.data;
            return;
        }
        this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL).subscribe(
            (data) => {
                this._catalogueRepo.customersSource$.next({ data }); // * Update service.
                this.customers = data;
                this.getBeneficiaryInfo();
            }
        );
    }

    getBeneficiaryInfo() {
        if (this.paymentMethod.value === this.methods[1] && !!this.payee.value) {
            const beneficiary = this.getPartnerById(this.payee.value);
            if (!!beneficiary) {
                this.beneficiaryName.setValue(beneficiary.partnerNameEn);
                this.bankAccountNo.setValue(beneficiary.bankAccountNo);
                if (beneficiary.bankName?.trim().length > 0) {
                    this.bankName.setValue(beneficiary.bankName);
                } else {
                    this.bankName.setValue(beneficiary.bankAccountName);
                }
            }
        } else {
            this.beneficiaryName.setValue(null);
            this.bankAccountNo.setValue(null);
            this.bankName.setValue(null);
        }
    }
    
    getPartnerById(id: string) {
        const partner: Partner = !this.customers ? null : this.customers.find((p: Partner) => p.id === id);
        return partner || null;
    }

}
