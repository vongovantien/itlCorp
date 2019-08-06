import { Component } from '@angular/core';
import { AppPage } from 'src/app/app.base';

@Component({
    selector: 'app-advance-payment-new',
    templateUrl: './add-new-advance-payment.component.html',
    styleUrls: ['./add-new-advance-payment.component.sass']
})

export class AdvancePaymentAddNewComponent extends AppPage {
    constructor() {
        super();
    }

    ngOnInit() { }
}