import {  Component, Output , EventEmitter} from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SystemConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Currency, Customer, User } from '@models';
import { Store } from '@ngrx/store';
import {CatalogueRepo, SystemRepo } from '@repositories';
import { GetCatalogueCurrencyAction, getCatalogueCurrencyState, IAppState } from '@store';
import { Observable } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'customer-payment-form-create-receipt',
    templateUrl: './form-create-receipt.component.html',
})
export class ARCustomerPaymentFormCreateReceiptComponent extends AppForm {
    @Output() onChangeCurrency: EventEmitter<Currency> = new EventEmitter<Currency>();
    form: FormGroup;
    customerName: AbstractControl;
    date: AbstractControl;
    paymentReferenceNo: AbstractControl;
    salesMan: AbstractControl;
    agreement: AbstractControl;
    paidAmount: AbstractControl;
    type: AbstractControl;
    cusAdvanceAmount: AbstractControl;
    finalPaidAmount: AbstractControl;
    balance: AbstractControl;
    currency: AbstractControl;
    paymentDate: AbstractControl;
    exchangeRate: AbstractControl;
    bankAcountNo: AbstractControl;
    currencyList: Currency[];
    constructor(
        private _fb: FormBuilder,
        private _store: Store<IAppState>,
    ) {
        super();
    }
    ngOnInit() {
        this._store.dispatch(new GetCatalogueCurrencyAction());
        this.initFormSettlement();
        this.getCurrency();

    }
    initFormSettlement() {
        this.form = this._fb.group({
            'customerName': [],
            'date': [{ startDate: new Date(), endDate: new Date() }, Validators.required],
            'paymentReferenceNo': [],
            'salesMan': [],
            'agreement': [],
            'paidAmount': [],
            'type': [],
            'cusAdvanceAmount': [],
            'finalPaidAmount': [],
            'balance': [],
            'currency': [],
            'paymentDate': [],
            'exchangeRate': [],
            'bankAcountNo': [],
        });
        this.customerName = this.form.controls['customerName'];
        this.date = this.form.controls['date'];
        this.paymentReferenceNo = this.form.controls['paymentReferenceNo'];
        this.salesMan = this.form.controls['salesMan'];
        this.agreement = this.form.controls['agreement'];
        this.paidAmount = this.form.controls['paidAmount'];
        this.type = this.form.controls['type'];
        this.cusAdvanceAmount = this.form.controls['cusAdvanceAmount'];
        this.finalPaidAmount = this.form.controls['finalPaidAmount'];
        this.balance = this.form.controls['balance'];
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

}
