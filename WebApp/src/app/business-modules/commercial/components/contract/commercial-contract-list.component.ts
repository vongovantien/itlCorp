import { Component, OnInit, ViewChild, Input } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { throwIfEmpty } from 'rxjs/operators';
import { Router } from '@angular/router';
import { FormContractComponent } from 'src/app/business-modules/share-commercial-catalogue/components/form-contract.component';
import { Contract } from 'src/app/shared/models/catalogue/catContract.model';

@Component({
    selector: 'commercial-contract-list',
    templateUrl: './commercial-contract-list.component.html',
})
export class CommercialContractListComponent extends AppList implements OnInit {
    @ViewChild(FormContractComponent, { static: false }) formContract: FormContractComponent;
    @Input() partnerId: string;
    contracts: Contract[] = [];
    selectecContract: any; // TODO: implement model.

    constructor(private _router: Router) {
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

    gotoCreateContract() {
        this._router.navigate([`home/commercial/customer//${this.partnerId}/contract/new`]);
    }



}
