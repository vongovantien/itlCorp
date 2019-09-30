import { Component } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';

@Component({
    selector: 'copy-settlement-popup',
    templateUrl: './copy-settlement.popup.html',
})
export class SettlementFormCopyPopupComponent extends PopupBase {
    constructor() {
        super();
    }

    ngOnInit(): void { }

    submitCopyCharge() {

    }
}
