import { Component } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'app-sea-fcl-export-bill-detail',
    templateUrl: './sea-fcl-export-bill-detail.component.html'
})
export class SeaFclExportBillDetailComponent extends AppList {
    housebills: any[] = [];
    constructor() {
        super();
    }
}
