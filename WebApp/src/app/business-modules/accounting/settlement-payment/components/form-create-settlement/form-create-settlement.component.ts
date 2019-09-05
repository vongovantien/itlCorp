import { Component } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { User } from 'src/app/shared/models';
import { BaseService } from 'src/app/shared/services';
import { AbstractControl, FormGroup, FormBuilder } from '@angular/forms';

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

    constructor(
        private _baseService: BaseService,
        private _fb: FormBuilder
    ) {
        super();
    }

    ngOnInit() { 
        this.form = this._fb.group({
            'settlementNo': [],
            'requester': [],
            'requestDate': [],
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

    }   

    getUserLogged() {
        this.userLogged = this._baseService.getUserLogin() || 'admin';
        this.requester.setValue(this.userLogged.id);
    }
}
