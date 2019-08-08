import { Component } from '@angular/core';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'adv-payment-form-search',
    templateUrl: './form-search-advance-payment.component.html'
})

export class AdvancePaymentFormsearchComponent extends AppForm {

    formSearch: FormGroup;
    referenceNo: AbstractControl;
    requester: AbstractControl;
    requestDate: AbstractControl;
    modifiedDate: AbstractControl;
    statusApproval: AbstractControl;
    statusPayment: AbstractControl;
    paymentMethod: AbstractControl;

    statusApprovals: CommonInterface.ICommonTitleValue[];
    statusPayments: CommonInterface.ICommonTitleValue[];
    paymentMethods: CommonInterface.ICommonTitleValue[] = [];

    constructor(
        private _fb: FormBuilder
    ) {
        super();
    }

    ngOnInit() {
        this.initForm();
        this.initDataInform();
    }

    initForm() {
        this.formSearch = this._fb.group({
            referenceNo: [],
            requester: [],
            requestDate: [new Date()],
            modifiedDate: [new Date()],
            statusApproval: [],
            statusPayment: [],
            paymentMethod: []
        });

        this.referenceNo = this.formSearch.controls['referenceNo'];
        this.requester = this.formSearch.controls['requester'];
        this.requestDate = this.formSearch.controls['requestDate'];
        this.modifiedDate = this.formSearch.controls['modifiedDate'];
        this.statusApproval = this.formSearch.controls['statusApproval'];
        this.statusPayment = this.formSearch.controls['statusPayment'];
        this.paymentMethod = this.formSearch.controls['paymentMethod'];
    }

    initDataInform() {
        this.statusApprovals = this.getStatusApproval();
        // this.statusApproval.setValue(this.statusApprovals[0]);

        this.statusPayments = this.getStatusPayment();
        // this.statusPayment.setValue(this.statusPayments[0]);

        this.paymentMethods = this.getMethod();
        // this.paymentMethod.setValue(this.paymentMethods[0]);
    }

    onSubmit(form: FormGroup) {
        console.log(form.value);
    }

    getStatusApproval(): CommonInterface.ICommonTitleValue[] {
        return [
            { title: 'New', value: 'New' },
            { title: 'Leader Approved', value: 'LeaderApproved' },
            { title: 'Department Manager Approved', value: 'New' },
            { title: 'NeAccountant Manager Approvedw', value: 'New' },
            { title: 'Done ', value: 'New'},
            { title: 'Denied  ', value: 'New' },
        ];
    }

    getStatusPayment(): CommonInterface.ICommonTitleValue[] {
        return [
            { title: 'Settled', value: 'Settled' },
            { title: 'Not Settled ', value: 'NotSettled' },
        ];
    }

    getMethod(): CommonInterface.ICommonTitleValue[] {
        return [
            { title: 'Cash', value: 'Cash' },
            { title: 'Bank Transer', value: 'Bank' },
        ];
    }
}
