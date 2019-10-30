import { Component } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Router } from '@angular/router';
import { CsTransactionDetail } from 'src/app/shared/models/document/csTransactionDetail';
import { DocumentationRepo } from 'src/app/shared/repositories';



@Component({
    selector: 'app-sea-fcl-import-management',
    templateUrl: './sea-fcl-import-management.component.html',
})
export class SeaFCLImportManagementComponent extends AppList {
    tabs: any[] = [
        { title: 'Shipment Detail', content: 'Dynamic content 1' },
        { title: 'Dynamic Title 2', content: 'Dynamic content 2' },
        { title: 'Dynamic Title 3', content: 'Dynamic content 3', removable: true }
    ];
    headers: CommonInterface.IHeaderTable[];
    houseBill: CsTransactionDetail[] = [];
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
