import { Component, OnInit, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { FormContractPopupComponent } from 'src/app/business-modules/share-commercial-catalogue/components/form-contract.popup';
import { throwIfEmpty } from 'rxjs/operators';

@Component({
    selector: 'commercial-contract-list',
    templateUrl: './commercial-contract-list.component.html',
})
export class CommercialContractListComponent extends AppList implements OnInit {
    @ViewChild(FormContractPopupComponent, { static: false }) popupContract: FormContractPopupComponent;
    contracts: any[] = []; // TODO: implement model.
    selectecContract: any; // TODO: implement model.

    constructor() {
        super();
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Salesman', field: 'username', sortable: true },
            { title: 'Contract No', field: 'username', sortable: true },
            { title: 'Contract Type', field: 'username', sortable: true },
            { title: 'Service', field: 'username', sortable: true },
            { title: 'Effective Date', field: 'username', sortable: true },
            { title: 'Expired Date', field: 'username', sortable: true },
            { title: 'Status', field: 'status', sortable: true },
            { title: 'Office', field: 'officeName', sortable: true },
            { title: 'Company', field: 'companyName', sortable: true },
        ];

    }

    openPopupContract() {
        this.popupContract.show();
    }
}
