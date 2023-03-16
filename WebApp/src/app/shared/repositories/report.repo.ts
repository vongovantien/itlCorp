import { Injectable } from "@angular/core";
import { throwError } from "rxjs";
import { catchError, map } from "rxjs/operators";
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

    exportAccountingPLSheet(body: any) {
        return this._api.downloadfile(`${environment.HOST.REPORT_MANAGEMENT}/api/v1/vi/AccountingReport/ExportAccountingPlSheet`, body, null, null, 'response');
    }

    exportJobProfitAnalysis(body: any) {
        return this._api.downloadfile(`${environment.HOST.REPORT_MANAGEMENT}/api/v1/vi/AccountingReport/ExportJobProfitAnalysis`, body, null, null, 'response');
    }
    exportSummaryOfCostsIncurred(body: any) {
        return this._api.downloadfile(`${environment.HOST.REPORT_MANAGEMENT}/api/v1/vi/AccountingReport/exportSummaryOfCostsIncurred`, body, null, null, 'response');
    }

    exportSummaryOfRevenueIncurred(body: any) {
        return this._api.downloadfile(`${environment.HOST.REPORT_MANAGEMENT}/api/v1/vi/AccountingReport/exportSummaryOfRevenueIncurred`, body, null, null, 'response');
    }

    exportCostsByPartner(body: any) {
        return this._api.downloadfile(`${environment.HOST.REPORT_MANAGEMENT}/api/v1/vi/AccountingReport/exportSummaryOfCostsPartner`, body, null, null, 'response');
    }

    exportShipmentOverview(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.REPORT_MANAGEMENT}/api/v1/vi/GeneralReport/ExportShipmentOverview`, searchObject, null, null, 'response').pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportShipmentOverviewWithType(searchObject: any = {}, reportType: string) {
        return this._api.downloadfile(`${environment.HOST.REPORT_MANAGEMENT}/api/v1/vi/GeneralReport/ExportShipmentOverviewWithType`, searchObject, { reportType: reportType }, null, 'response').pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportStandardGeneralReport(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.REPORT_MANAGEMENT}/api/v1/vi/GeneralReport/ExportStandardGeneralReport`, searchObject, null, null, 'response').pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportEDocReport(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.REPORT_MANAGEMENT}/api/v1/vi/EDocReport/ExportEDocTemplateReport`, searchObject, null, null, 'response').pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }
}
