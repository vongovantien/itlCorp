import { Component, Output, EventEmitter, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Data } from '@angular/router';
import { SystemConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Currency, Customer, Partner, User } from '@models';
import { Store } from '@ngrx/store';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { GetCatalogueCurrencyAction, getCatalogueCurrencyState, IAppState } from '@store';
import { Moment } from 'moment';
import { Observable } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { AppForm } from 'src/app/app.form';
import { catAgreement } from 'src/app/shared/models/catalogue/catAgreement.model';

@Component({
    selector: 'customer-payment-form-create-receipt',
    templateUrl: './form-create-receipt.component.html',
})
export class ARCustomerPaymentFormCreateReceiptComponent extends AppForm implements OnInit {
    @Output() onChangeCurrency: EventEmitter<Currency> = new EventEmitter<Currency>();
    form: FormGroup;
    selected: { start: Moment, end: Moment };
    customerId: AbstractControl; // load partner
    date: AbstractControl;
    paymentReferenceNo: AbstractControl;
    agreement: AbstractControl;
    paidAmount: AbstractControl;
    methods: CommonInterface.ICommonTitleValue[];
    userLogged: SystemInterface.IClaimUser;
    type: AbstractControl;
    cusAdvanceAmount: AbstractControl;
    finalPaidAmount: AbstractControl;
    balance: AbstractControl;
    paymentMethod: AbstractControl;
    currency: AbstractControl;
    paymentDate: AbstractControl;
    exchangeRate: AbstractControl;
    bankAcountNo: AbstractControl;
    currencyList: Currency[];

    customers: Observable<Partner[]>; /// partner = customer
    agreements: Observable<catAgreement[]>;
    $agreements: Observable<any>;
    username: AbstractControl;
    paymentMethods: string[] = ['Cash', 'Bank Transfer'];
    displayFieldsPartner: CommonInterface.IComboGridDisplayField[] = [   // load display fiels partner
        { field: 'accountNo', label: 'ID' },
        { field: 'shortName', label: 'Name ABBR' },
        { field: 'partnerNameVn', label: 'Name Local' },
        { field: 'taxCode', label: 'Tax Code' },

    ];
    displayFieldAgreement: CommonInterface.IComboGridDisplayField[] = [
        { field: 'saleManName', label: 'Salesman' },
        { field: 'contractNo', label: 'Contract No' },
        { field: 'contractType', label: 'Contract Type' },

    ];
    constructor(
        private _fb: FormBuilder,
        private _store: Store<IAppState>,
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo

    ) {
        super();
    }
    ngOnInit() {

        this.initFormSettlement();
        this.getCurrency();
        this.customers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL); // khai báo load part lên display

    }
    initFormSettlement() {
        this.form = this._fb.group({
            'customerId': [], // khai báo list partner trong form-group
            'date': [{ startDate: new Date(), endDate: new Date() }, Validators.required],
            'paymentReferenceNo': [],
            'agreement': [],
            'paidAmount': [],
            'type': [],
            'cusAdvanceAmount': [],
            'finalPaidAmount': [],
            'balance': [],
            'paymentMethod': [this.paymentMethods[1], this.paymentMethods[2]],
            'currency': [],
            'paymentDate': [],
            'exchangeRate': [],
            'bankAcountNo': [],
        });
        this.customerId = this.form.controls['customerId']; // partner 
        this.date = this.form.controls['date'];
        this.paymentReferenceNo = this.form.controls['paymentReferenceNo'];
        this.agreement = this.form.controls['agreement'];
        this.paidAmount = this.form.controls['paidAmount'];
        this.type = this.form.controls['type'];
        this.cusAdvanceAmount = this.form.controls['cusAdvanceAmount'];
        this.finalPaidAmount = this.form.controls['finalPaidAmount'];
        this.balance = this.form.controls['balance'];
        this.paymentMethod = this.form.controls['paymentMethod'];
        this.currency = this.form.controls['currency'];
        // this.currency.valueChanges.pipe(
        //     // tslint:disable-next-line:no-any
        //     map((data: any) => data)
        // ).subscribe((value: Currency) => {
        //     if (!!value) {
        //         this.onChangeCurrency.emit(value);
        //     }
        // });
        this.paymentDate = this.form.controls['paymentDate'];
        this.exchangeRate = this.form.controls['exchangeRate'];
        this.bankAcountNo = this.form.controls['bankAcountNo'];
    }
    getCurrency() {
        this._store.select(getCatalogueCurrencyState)
            .pipe(catchError(this.catchError))
            .subscribe(
                // tslint:disable-next-line:no-any
                (data: any) => {
                    this.currencyList = data || [];
                    this.currency.setValue("VND");
                },
            );
    }


    // tslint:disable-next-line:no-any
    onSelectDataFormInfo(data: any, type: string) {  // lấy data partner 
        switch (type) {
            case 'partner':
                this.customerId.setValue((data as Partner).id);
                // 
                this.getAgreements({ status: true, partnerId: this.customerId.value });
                break;
            default:
                break;
        }
    }



    getAgreements(body) {
        this._catalogueRepo.getAgreement(body)
            .pipe(catchError(this.catchError))
            .subscribe(
                data => {
                    this.agreements = data;

                }
            );
    }

}
