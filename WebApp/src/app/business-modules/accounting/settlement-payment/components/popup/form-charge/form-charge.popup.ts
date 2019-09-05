import { Component } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';

@Component({
    selector: 'form-charge-popup',
    templateUrl: './form-charge.popup.html'
})

export class SettlementFormChargePopupComponent extends PopupBase {
    constructor() {
        super();
    }

    ngOnInit() { }
}
