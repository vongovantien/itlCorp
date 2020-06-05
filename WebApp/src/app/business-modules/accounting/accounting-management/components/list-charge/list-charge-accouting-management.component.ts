import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'list-charge-accounting-management',
    templateUrl: './list-charge-accounting-management.component.html',
})
export class AccountingManagementListChargeComponent extends AppList implements OnInit {
    constructor() {
        super();
    }

    ngOnInit(): void { }
}
