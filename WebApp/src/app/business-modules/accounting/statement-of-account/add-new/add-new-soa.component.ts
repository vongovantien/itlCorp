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

    charges: any[] = [];
    headers: any = null;

    isCollapsed: boolean = true;

    isCheckAllCharge: boolean = false;

    constructor(
        private _sysRepo: SystemRepo,
        private _globalState: GlobalState
    ) {
        super();
    }

    ngOnInit() {
        this.headers = [
            { title: 'Charge Code', field: 'code', sortable: true },
            { title: 'Charge Name', field: 'type', sortable: true },
            { title: 'JobID', field: 'customerName', sortable: true },
            { title: 'HBL', field: 'name', sortable: true },
            { title: 'MBL', field: 'total', sortable: true },
            { title: 'Custom No', field: 'date', sortable: true },
            { title: 'Debit', field: 'createdDate', sortable: true },
            { title: 'Credit', field: 'status', sortable: true },
            { title: 'Currency', field: 'action', sortable: true },
            { title: 'Invoice No', field: 'action', sortable: true },
            { title: 'Services Date', field: 'action', sortable: true },
            { title: 'Note', field: 'action', sortable: true },
        ];
        this.getBasicData();
        this.charges = this.getListCharge();
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
                // complete
                () => {
                }
            );
    }

    getListCharge() {
        const results: any[] = [];
        const data = { code: '', name: 'Name', jobId: 'XXXX', hbl: 'HBL no', mbl: 'MBL no', customNo: 'Customer no', debit: '12313', credit: '456', currency: 'VND', invoiceNo: '123', serviceDate: 'dd/mm/yyyy', note: 'lorem 10', isSelected: false };

        for (let index = 1; index < 50; index++) {
            results.push(Object.assign({}, data, {code: Math.random()}));
        }

        return results;
    }

    onChangeCheckBoxCharge($event: Event) {
        this.isCheckAllCharge = this.charges.every((item: any) => item.isSelected);
    }

    checkUncheckAllCharge() {
        for (const charge of this.charges) {
            charge.isSelected = this.isCheckAllCharge;
          }
    }

    onCreateSOA() {

    }

    onSearchCharge(data: any) {
    }
}
