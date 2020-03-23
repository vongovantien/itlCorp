import { AppList } from "src/app/app.list";
import { Component } from "@angular/core";
import { NgProgress } from "@ngx-progressbar/core";
import { ToastrService } from "ngx-toastr";

@Component({
    selector: 'app-sale-report',
    templateUrl: './sale-report.component.html',
})
export class SaleReportComponent extends AppList {
    constructor(
        private _progressService: NgProgress,
        private _toastService: ToastrService,
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
    }

    onSearchSaleReport(data: any) {
        console.log(data);
        this.dataSearch = data;
    }

}