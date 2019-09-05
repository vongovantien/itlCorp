import { Component } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';

@Component({
    selector: 'existing-charge-popup',
    templateUrl: './existing-charge.popup.html'
})

export class SettlementExistingChargePopupComponent extends PopupBase {
    constructor() {
        super();
    }

    ngOnInit() { }
}
