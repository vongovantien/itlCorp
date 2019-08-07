import { Component, ViewChild } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { AccoutingRepo } from 'src/app/shared/repositories';
import { catchError, map } from 'rxjs/operators';
import { CustomDeclaration } from 'src/app/shared/models';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';

@Component({
    selector: 'adv-payment-add-popup',
    templateUrl: './add-advance-payment.popup.html'
})

export class AdvancePaymentAddPopupComponent extends PopupBase {

    @ViewChild(ConfirmPopupComponent, {static: false}) confirmPopup: ConfirmPopupComponent;

    types: CommonInterface.ICommonTitleValue[];
    selectedType: CommonInterface.ICommonTitleValue;

    customDeclarations: CustomDeclaration[];
    form: FormGroup;
    description: AbstractControl;
    amount: AbstractControl;
    currency: AbstractControl;
    type: AbstractControl;
    note: AbstractControl;
    shipment: AbstractControl;
    customNo: AbstractControl;


    constructor(
        private _fb: FormBuilder,
        private _accoutingRepo: AccoutingRepo
    ) {
        super();
    }

    ngOnInit() {
        this.initForm();
        this.initBasicData();
        this.getCustomNo();
    }

    initBasicData() {
        this.types = [
            { title: 'Norm', value: 'Norm' },
            { title: 'Invoice', value: 'Invoice' },
            { title: 'Other', value: 'other' },
        ];

        this.type.setValue(this.types[0]);
    }

    initForm() {
        this.form = this._fb.group({
            'description': [, Validators.compose([
                Validators.pattern(/^[\w '_"/*\\\.,-]*$/)
            ])],
            'amount': [],
            'note': [],
            'customNo': [],
            'shipment': [],
            'type': [],
            'currency': [],
        });

        this.description = this.form.controls['description'];
        this.amount = this.form.controls['amount'];
        this.note = this.form.controls['note'];
        this.customNo = this.form.controls['customNo'];
        this.type = this.form.controls['type'];
        this.currency = this.form.controls['currency'];
    }

    onSubmit() {
        console.log(this.form.value);
    }

    getCustomNo() {
        this._accoutingRepo.getListCustomsDeclaration()
            .pipe(
                catchError(this.catchError),
                map((response: any[]) => response.map( (item: CustomDeclaration) => new CustomDeclaration(item))),
            )
            .subscribe(
                (res: any) => {
                    this.customDeclarations = res || [];
                    console.log(this.customDeclarations);
                },
                (errors: any) => { },
                () => { },
            );
    }

    onCancel() {
        this.confirmPopup.show();
    }

    onSubmitExit() {
        this.confirmPopup.hide();
        this.hide();
    }
}

