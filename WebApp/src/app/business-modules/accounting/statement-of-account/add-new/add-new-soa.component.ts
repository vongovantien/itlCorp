import { Component, ViewChild } from '@angular/core';

import { StatementOfAccountAddChargeComponent } from '../components/poup/add-charge/add-charge.popup';
import { SystemRepo } from 'src/app/shared/repositories';
import { GlobalState } from 'src/app/global-state';
import { AppList } from 'src/app/app.list';

import { takeUntil } from 'rxjs/operators';
import { forkJoin } from 'rxjs';
@Component({
    selector: 'app-statement-of-account-new',
    templateUrl: './add-new-soa.component.html',
    styleUrls: ['./add-new-soa.component.scss']
})
export class StatementOfAccountAddnewComponent extends AppList {

    @ViewChild(StatementOfAccountAddChargeComponent, { static: false }) addChargePopup: StatementOfAccountAddChargeComponent;
    isCollapsed: boolean = true;
    headers: any = null;
    
    constructor(
        private _sysRepo: SystemRepo,
        private _globalState: GlobalState
    ) {
        super();
    }

    ngOnInit() {
        this.headers = [
            { title: '', field: '' },
            { title: 'Charge Code', field: 'code', sortable: true },
            { title: 'Charge Name', field: 'type', sortable: true },
            { title: 'JobID', field: 'customerName', sortable: true },
            { title: 'HBL', field: 'name', sortable: true },
            { title: 'MBL', field: 'total', sortable: true },
            { title: 'Custom No', field: 'date', sortable: true },
            { title: 'Debit', field: 'createdDate', sortable: true },
            { title: 'Credit', field: 'status', sortable: true },
            { title: 'Currency', field: 'action',  sortable: true},
            { title: 'Invoice No', field: 'action',  sortable: true},
            { title: 'Services Date', field: 'action',  sortable: true},
            { title: 'Note', field: 'action',  sortable: true},
        ];
        this.getBasicData();
    }

    addCharge() {
        this.addChargePopup.show();
    }

    getBasicData() {
        forkJoin([
            this._sysRepo.getListCurrency(1, 20),
            this._sysRepo.getListSystemUser()
        ])
        .pipe(takeUntil(this.ngUnsubscribe))
        .subscribe(
            ([dataCurrency, dataSystemUser]: any) => {
                this._globalState.notifyDataChanged('currency', dataCurrency);
                this._globalState.notifyDataChanged('system-user', dataSystemUser);
            },
            (err: any) => {
            },
            //complete
            () => {
            }
        );
    }
}
