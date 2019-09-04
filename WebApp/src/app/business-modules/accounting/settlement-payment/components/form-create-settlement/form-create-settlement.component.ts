import { Component } from '@angular/core';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'settle-payment-form-create',
    templateUrl: './form-create-settlement.component.html'
})

export class SettlementFormCreateComponent extends AppForm {
    constructor() {
        super();
    }

    ngOnInit() { }
}