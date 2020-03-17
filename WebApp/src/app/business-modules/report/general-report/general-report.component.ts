import { AppList } from "src/app/app.list";
import { Component } from "@angular/core";
import { SortService } from "@services";
import { NgProgress } from "@ngx-progressbar/core";
import { ToastrService } from "ngx-toastr";

@Component({
    selector: 'app-general-report',
    templateUrl: './general-report.component.html',
})
export class GeneralReportComponent extends AppList {
    headers: CommonInterface.IHeaderTable[];
    dataList: any[] = [];

    constructor(
        private _sortService: SortService,
        private _progressService: NgProgress,
        private _toastService: ToastrService,
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this.headers = [
            { title: 'No.', field: 'no', sortable: true },
            { title: 'Job ID', field: 'jobNo', sortable: true },
            { title: 'MBL/MAWB', field: 'mawb', sortable: true },
            { title: 'HBL/HAWB', field: 'hawb', sortable: true },
            { title: 'Customer', field: 'customerName', sortable: true },
            { title: 'Carrier', field: 'carrierName', sortable: true },
            { title: 'Agent', field: 'agentName', sortable: true },
            { title: 'Service Date', field: 'serviceDate', sortable: true },
            { title: 'Route', field: 'route', sortable: true },
            { title: 'Qty', field: 'qty', sortable: true },
            { title: 'Revenue', field: 'revenue', sortable: true },
            { title: 'Cost', field: 'cost', sortable: true },
            { title: 'Profit', field: 'profit', sortable: true },
            { title: 'OBH', field: 'obh', sortable: true },
            { title: 'P.I.C', field: 'picName', sortable: true },
            { title: 'Salesman', field: 'salesManName', sortable: true },
            { title: 'Service', field: 'serviceName', sortable: true },
        ];
    }

}