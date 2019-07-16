import { Component, ViewChild } from '@angular/core';
import { AppPage } from 'src/app/app.base';
import { StatementOfAccountAddChargeComponent } from '../components/poup/add-charge/add-charge.popup';

@Component({
    selector: 'app-statement-of-account-edit',
    templateUrl: './edit-soa.component.html',
    styleUrls: ['./edit-soa.component.scss']
})
export class StatementOfAccountEditComponent extends AppPage {
    @ViewChild(StatementOfAccountAddChargeComponent, { static: false }) addChargePopup: StatementOfAccountAddChargeComponent;
    constructor() {
        super();
    }

    ngOnInit() {
    }

    addCharge() {
        this.addChargePopup.show();
    }
}
