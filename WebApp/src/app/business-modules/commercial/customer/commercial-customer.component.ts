import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'app-commercial-customer',
    templateUrl: './commercial-customer.component.html',
})
export class CommercialCustomerComponent extends AppList implements OnInit {
    constructor() {
        super();
    }

    ngOnInit(): void { }
}

