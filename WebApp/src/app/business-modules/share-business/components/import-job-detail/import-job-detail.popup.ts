import { Component, OnInit } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';

@Component({
    selector: 'import-job-detail-popup',
    templateUrl: 'import-job-detail.popup.html'
})

export class ShareBusinessImportJobDetailPopupComponent extends PopupBase implements OnInit {
    headers: CommonInterface.IHeaderTable[];
    constructor() {
        super();

    }

    ngOnInit() {
        this.headers = [
            { title: 'Job ID', field: 'jobId', sortable: true },
            { title: 'MBL No', field: 'mawb', sortable: true },
            { title: 'Supplier(Shipping Line)', field: 'supplierName', sortable: true },
            { title: 'Shipment Date', field: 'etd', sortable: true }
        ];
    }

    onCancel() {
        this.hide();
    }
}
