import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'unlock-accounting',
    templateUrl: './unlock-accouting.component.html'
})

export class UnlockAccountingComponent extends AppForm implements OnInit {
    constructor() {
        super();
    }

    ngOnInit() { }

    unlockPaymentRequest() {

    }
}