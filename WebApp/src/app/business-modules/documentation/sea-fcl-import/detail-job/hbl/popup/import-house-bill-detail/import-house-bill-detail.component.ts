import { Component, OnInit } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import moment from 'moment';

@Component({
    selector: 'popup-import-house-bill-detail',
    templateUrl: './import-house-bill-detail.component.html',
    styleUrls: ['./import-house-bill-detail.component.scss']
})
export class ImportHouseBillDetailComponent extends PopupBase {
    searchFilters: Array<string> = ['MBL', 'Customer', 'Saleman'];
    searchFilterActive = ['MBL'];
    disabled: boolean = false;
    maxDate: moment.Moment = moment();
    selectedRange: any;
    selectedDate: any;
    headers: CommonInterface.IHeaderTable[];
    dataSearch: any = {};
    ranges: any = {
        Today: [moment(), moment()],
        Yesterday: [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
        'Last 7 Days': [moment().subtract(6, 'days'), moment()],
        'Last 30 Days': [moment().subtract(29, 'days'), moment()],
        'This Month': [moment().startOf('month'), moment().endOf('month')],
        'Last Month': [
            moment()
                .subtract(1, 'month')
                .startOf('month'),
            moment()
                .subtract(1, 'month')
                .endOf('month')
        ]
    };
    constructor() {
        super();
    }

    ngOnInit() {
        this.headers = [
            { title: 'HBL No', field: 'hwbno', sortable: true },
            { title: 'MBL No', field: 'mawb', sortable: true },
            { title: 'Customer', field: 'customerName', sortable: true },
            { title: 'SaleMan', field: 'saleManName', sortable: true },
            { title: 'Shipment Date', field: 'etd', sortable: true }
        ];
    }
    onCancel() {
        this.hide();
    }



}
