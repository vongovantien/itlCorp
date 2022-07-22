import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'form-search-prepaid-payment',
    templateUrl: './form-search-prepaid-payment.component.html',
})
export class ARPrePaidPaymentFormSearchComponent extends AppForm implements OnInit {

    constructor() {
        super();
    }

    ngOnInit(): void { }
}
