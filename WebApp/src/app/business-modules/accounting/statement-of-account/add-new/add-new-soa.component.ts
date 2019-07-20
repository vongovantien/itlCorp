import { Component, ViewChild } from '@angular/core';

import { StatementOfAccountAddChargeComponent } from '../components/poup/add-charge/add-charge.popup';
import { SystemRepo } from 'src/app/shared/repositories';
import { GlobalState } from 'src/app/global-state';
import { AppList } from 'src/app/app.list';

import { takeUntil } from 'node_modules/rxjs/operators';
import { forkJoin } from 'node_modules/rxjs';
import { SortService } from 'src/app/shared/services';
@Component({
    selector: 'app-statement-of-account-new',
    templateUrl: './add-new-soa.component.html',
    styleUrls: ['./add-new-soa.component.scss'],
})
export class StatementOfAccountAddnewComponent extends AppList {

    @ViewChild(StatementOfAccountAddChargeComponent, { static: false }) addChargePopup: StatementOfAccountAddChargeComponent;

    charges: any[] = [];
    headers: any = null;

    isCollapsed: boolean = true;

    isCheckAllCharge: boolean = false;

    constructor(
        private _sysRepo: SystemRepo,
        private _globalState: GlobalState,
        private _sortService: SortService
    ) {
        super();
        this.requestList = this.sortLocal;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Charge Code', field: 'code', sortable: true },
            { title: 'Charge Name', field: 'name', sortable: true },
            { title: 'JobID', field: 'jobId', sortable: true },
            { title: 'HBL', field: 'hbl', sortable: true },
            { title: 'MBL', field: 'mbl', sortable: true },
            { title: 'Custom No', field: 'custom', sortable: true },
            { title: 'Debit', field: 'debit', sortable: true },
            { title: 'Credit', field: 'credit', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'Invoice No', field: 'invoice', sortable: true },
            { title: 'Services Date', field: 'serviceDate', sortable: true },
            { title: 'Note', field: 'action', sortable: true },
        ];
        this.getBasicData();
        this.getListCharge();
    }

    addCharge() {
        this.addChargePopup.show();
        this._globalState.notifyDataChanged('system-user', []);
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

        for (let index = 1; index < 10; index++) {
            results.push(Object.assign({}, data, { code: Math.floor(Math.random() * 10 + 1) }, { jobId: Math.floor(Math.random() * 20 + 1) }));
        }

        this.charges = results;
    }

    sortLocal(sortField?: string, order?: boolean) {
        console.log(this.sort, this.order);
        this.charges = this._sortService.sort(this.charges, sortField, order);
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
