import { Component } from '@angular/core';
import { AppPage } from 'src/app/app.base';

@Component({
    selector: 'app-settle-payment-new',
    templateUrl: './add-settle-payment.component.html'
})

export class SettlementPaymentAddNewComponent extends AppPage {
    constructor() {
        super();
    }

    ngOnInit() { }
}
