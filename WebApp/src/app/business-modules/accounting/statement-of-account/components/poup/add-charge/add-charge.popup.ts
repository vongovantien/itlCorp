import { Component } from '@angular/core';
import { AppPage } from 'src/app/app.base';

@Component({
    selector: 'soa-add-charge-popup',
    templateUrl: './add-charge.popup.html',
    styleUrls: ['./add-charge.popup.scss']
})
export class StatementOfAccountAddChargeComponent extends AppPage {
    constructor() {
        super();
    }

    ngOnInit(): void { }
}
