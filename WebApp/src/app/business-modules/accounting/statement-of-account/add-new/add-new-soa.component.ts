import { Component, ViewChild } from '@angular/core';

import { AppPage, IComboGirdConfig } from 'src/app/app.base';
import { StatementOfAccountAddChargeComponent } from '../components/poup/add-charge/add-charge.popup';
@Component({
    selector: 'app-statement-of-account-new',
    templateUrl: './add-new-soa.component.html',
    styleUrls: ['./add-new-soa.component.scss']
})
export class StatementOfAccountAddnewComponent extends AppPage {

    @ViewChild(StatementOfAccountAddChargeComponent, { static: false }) addChargePopup: StatementOfAccountAddChargeComponent;
    isCollapsed: boolean = true;

    constructor() {
        super();
    }

    ngOnInit() {

    }

    addCharge() {
        this.addChargePopup.show();
    }
}
