import { Injectable } from "@angular/core";
import { environment } from "src/environments/environment";
import { ApiService } from "../services";

@Injectable({ providedIn: 'root' })
export class ReportManagementRepo {
    private VERSION: string = 'v1';
    constructor(protected _api: ApiService) {

    }

    previewCombinationSalesReport(body: any) {
        return this._api.post(`${environment.HOST.REPORT_MANAGEMENT}/api/${this.VERSION}/en-US/SaleReport/CombinationSaleReport`, body);
    }

    previewSaleKickBackReport(body: any) {
        return this._api.post(`${environment.HOST.REPORT_MANAGEMENT}/api/${this.VERSION}/en-US/SaleReport/SaleKickBackReport`, body);
    }

    previewSaleDepartmentReport(body: any) {
        return this._api.post(`${environment.HOST.REPORT_MANAGEMENT}/api/${this.VERSION}/en-US/SaleReport/DepartSaleReport`, body);
    }

    previewSaleMonthlyReport(body: any) {
        return this._api.post(`${environment.HOST.REPORT_MANAGEMENT}/api/${this.VERSION}/en-US/SaleReport`, body);
    }

    previewSaleQuaterReport(body: any) {
        return this._api.post(`${environment.HOST.REPORT_MANAGEMENT}/api/${this.VERSION}/en-US/SaleReport/QuaterSaleReport`, body);
    }

    previewSaleSummaryReport(body: any) {
        return this._api.post(`${environment.HOST.REPORT_MANAGEMENT}/api/${this.VERSION}/en-US/SaleReport/SummarySaleReport`, body);
    }

    getGeneralReport(page?: number, size?: number, body: any = {}) {
        return this._api.post(`${environment.HOST.REPORT_MANAGEMENT}/api/${this.VERSION}/en-US/GeneralReport/GetDataGeneralReport`, body, {
            page: '' + page,
            size: '' + size
        });
    }
}
