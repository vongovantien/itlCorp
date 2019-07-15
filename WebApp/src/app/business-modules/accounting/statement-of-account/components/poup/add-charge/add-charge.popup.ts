import { Component } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';

@Component({
    selector: 'soa-add-charge-popup',
    templateUrl: './add-charge.popup.html',
    styleUrls: ['./add-charge.popup.scss']
})
export class StatementOfAccountAddChargeComponent extends PopupBase {
    constructor() {
        super();
    }

    ngOnInit(): void { }
}
