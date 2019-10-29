import { Component } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { Router } from '@angular/router';

@Component({
    selector: 'app-create-job-fcl-import',
    templateUrl: './create-job-fcl-import.component.html',
    styleUrls: ['./create-job-fcl-import.component.scss']
})
export class SeaFCLImportCreateJobComponent extends AppForm {
    constructor(
        protected _router: Router
    ) {
        super();
    }

    ngOnInit(): void { }

    gotoDetailJob() {
        this._router.navigate([`home/documentation/sea-fcl-import/${123}`], { queryParams: { tab: 'SHIPMENT' } });

    }
}
