import { Component } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Router } from '@angular/router';

@Component({
    selector: 'app-sea-fcl-import-management',
    templateUrl: './sea-fcl-import-management.component.html',
})
export class SeaFCLImportManagementComponent extends AppList {

    constructor(
        private _router: Router
    ) {
        super();
    }

    ngOnInit() {
    }

    gotoCreateJob() {
        this._router.navigate(['home/documentation/sea-fcl-import/new']);
    }

}
