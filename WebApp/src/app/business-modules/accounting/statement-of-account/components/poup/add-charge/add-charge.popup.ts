import { Component } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'soa-add-charge-popup',
    templateUrl: './add-charge.popup.html',
    styleUrls: ['./add-charge.popup.scss']
})
export class StatementOfAccountAddChargeComponent extends PopupBase {
    obhs: any = [];
    selectedOBH: any = null;

    types: any = [];
    selectedType: any = null;

    inSOAs: any[] = [];
    selectedInSOA: any = null;

    headers: CommonInterface.IHeaderTable[];
    sort: string = null;
    order: any = false;

    isCheckAllCharge: boolean  = false;
    constructor() {
        super();
    }

    ngOnInit() {
        this.headers = [
            { title: 'Charge Code', field: 'chargeCode', sortable: true },
            { title: 'Charge Name', field: 'chargeName', sortable: true },
            { title: 'JobID', field: 'jobId', sortable: true },
            { title: 'HBL', field: 'hbl', sortable: true },
            { title: 'MBL', field: 'mbl', sortable: true },
            { title: 'Custom No', field: 'customNo', sortable: true },
            { title: 'Debit', field: 'debit', sortable: true },
            { title: 'Credit', field: 'credit', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'Invoice No', field: 'invoiceNo', sortable: true },
            { title: 'Services Date', field: 'serviceDate', sortable: true },
            { title: 'Note', field: 'note', sortable: true },
        ];
        this.initBasicData();
     }

    initBasicData() {
        this.types = [
            { id: 1, text: 'All' },
            { text: 'Debit', id: 2 },
            { text: 'Credit', id: 3 },
        ];
        this.selectedType = this.types[0];

        this.obhs = [
            { text: 'Yes', id: 1 },
            { text: 'No', id: 2 }
        ];
        this.selectedOBH = this.obhs[1];

        this.inSOAs = [
            { text: 'Yes', id: 1 },
            { text: 'No', id: 2 }
        ];
        this.selectedInSOA = this.inSOAs[1];

    }

    setSortBy(sort?: string, order?: boolean): void {
        this.sort = sort ? sort : 'code';
        this.order = order;
    }

    sortClass(sort: string): string {
        if (!!sort) {
            let classes = 'sortable ';
            if (this.sort === sort) {
                classes += ('sort-' + (this.order ? 'asc' : 'desc') + ' ');
            }

            return classes;
        }
        return '';
    }

    sortBy(sort: string): void {
        if (!!sort) {
            this.setSortBy(sort, this.sort !== sort ? true : !this.order);
        }
    }

    checkUncheckAllCharge() {

    }

    onChangeCheckBoxCharge(data) {}

}
