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
    currencyList: any[];
    selectedRange: any;
    maxDate: any;
    constructor() {
        super();
    }

    ngOnInit() {
    }

    addCharge() {
        this.addChargePopup.show();
    }

    /**
    * ng2-select
    */
    private value: any = {};
    public selected(value: any): void {
        console.log('Selected value is: ', value);
    }

    public removed(value: any): void {
        console.log('Removed value is: ', value);
    }

    public typed(value: any): void {
        console.log('New search input: ', value);
    }

    public refreshValue(value: any): void {
        this.value = value;
    }
}
