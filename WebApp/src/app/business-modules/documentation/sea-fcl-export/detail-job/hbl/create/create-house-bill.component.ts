import { Component } from '@angular/core';
import { AppPage } from 'src/app/app.base';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'app-create-hbl-fcl-export',
    templateUrl: './create-house-bill.component.html'
})

export class SeaFCLExportCreateHBLComponent extends AppForm {
    constructor() {
        super();
    }

    ngOnInit() { }
}
