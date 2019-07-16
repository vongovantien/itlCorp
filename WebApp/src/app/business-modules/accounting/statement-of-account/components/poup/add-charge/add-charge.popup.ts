import { Component } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';

@Component({
    selector: 'soa-add-charge-popup',
    templateUrl: './add-charge.popup.html',
    styleUrls: ['./add-charge.popup.scss']
})
export class StatementOfAccountAddChargeComponent extends PopupBase {
    items: any[];

    constructor() {
        super();
    }

    ngOnInit(): void { }


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
