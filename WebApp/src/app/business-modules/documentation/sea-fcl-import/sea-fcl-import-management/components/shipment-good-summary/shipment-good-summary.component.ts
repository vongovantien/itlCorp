import { Component } from '@angular/core';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'shipment-good-summary',
    templateUrl: './shipment-good-summary.component.html',
})
export class SeaFCLImportShipmentGoodSummaryComponent extends AppForm {
    constructor() {
        super();
    }

    ngOnInit(): void { }
}
