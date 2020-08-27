import { Component, OnInit, ViewChild } from '@angular/core';
import { AppList, IPermissionBase } from 'src/app/app.list';

@Component({
    selector: 'app-commercial-potential-customer',
    templateUrl: './commercial-potential-customer.component.html',
})
export class CommercialPotentialCustomerComponent extends AppList implements OnInit, IPermissionBase {
    constructor(
    ) {
        super();

    }
    ngOnInit(): void {

    }
    checkAllowDetail(T: any): void {
        throw new Error("Method not implemented.");
    }
    checkAllowDelete(T: any): void {
        throw new Error("Method not implemented.");
    }

}

