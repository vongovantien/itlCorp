import { PopupBase } from "@app";
import { Component, Output, EventEmitter } from "@angular/core";
import { FormGroup, AbstractControl, FormBuilder } from "@angular/forms";
import { AccountingConstants } from "@constants";

@Component({
    selector: 'soa-payment-method-popup',
    templateUrl: './soa-payment-method.popup.html'
})
export class StatementOfAccountPaymentMethodComponent extends PopupBase {
    @Output() onApply: EventEmitter<string> = new EventEmitter<string>();
    paymentMethods: CommonInterface.INg2Select[] = AccountingConstants.PAYMENT_METHOD_2.map(i => ({ id: i.id, text: i.text }));
    formGroup: FormGroup;
    paymentMethod: AbstractControl;
    paymentMethodActive: any[] = [];
    constructor(
        private _fb: FormBuilder,
    ) {
        super();
    }

    ngOnInit() {
        this.paymentMethodActive = [this.paymentMethods[0]];
        this.formGroup = this._fb.group({
            paymentMethod: [this.paymentMethodActive]
        });
        this.paymentMethod = this.formGroup.controls['paymentMethod'];
    }

    apply() {
        this.hide();
        this.onApply.emit(this.paymentMethod.value[0].id);
    }

    closePopup() {
        this.hide();
    }
}