import { Component, OnInit } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';

@Component({
    selector: 'form-quick-update-receipt',
    templateUrl: './form-quick-update-receipt.popup.html',
})
export class ARCustomerPaymentFormQuickUpdateReceiptPopupComponent extends PopupBase implements OnInit {
    updateKey: string;
    constructor() {
        super();
    }

    ngOnInit(): void { }

    onConfirmSave() {

    }
}
