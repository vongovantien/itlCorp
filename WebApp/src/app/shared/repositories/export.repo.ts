import { Injectable } from '@angular/core';
import { ApiService } from '../services';
import { environment } from 'src/environments/environment';
import { map, catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ExportRepo {
    constructor(private _api: ApiService) {
    }

    exportCrystalReportPDF(data: any) {
        return this._api.postFormData(`${environment.HOST.EXPORT_CRYSTAL}`, `crystal=${JSON.stringify(data)}`);
    }

    exportCustomClearance(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Catalogue/CustomsDeclaration/ExportCustomClearance`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map(data => data)
        );
    }


    exportCompany(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/SystemReport/ExportCompany`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map(data => data)
        );
    }

    exportDepartment(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/SystemReport/ExportDepartment`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map(data => data)
        );
    }
    exportOffice(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/SystemReport/ExportOffice`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map(data => data)
        );
    }
    exportGroup(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/SystemReport/ExportGroup`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map(data => data)
        );
    }

    exportUser(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/SystemReport/ExportUser`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map(data => data)
        );
    }
    exportEManifest(hblId: string) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Documentation/ExportEManifest`, null, { hblid: hblId }).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportGoodDeclare(hblId: string) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Documentation/ExportGoodsDeclare`, null, { hblid: hblId }).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }
    exportDangerousGoods(hblId: string) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Documentation/ExportDangerousGoods`, null, { hblid: hblId }).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportPartner(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Catalogue/ExportPartnerData`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportCurrency(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Catalogue/ExportCurrency`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportPortIndex(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Catalogue/ExportPortIndex`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportCommodity(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Catalogue/ExportCommodityList`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportCommodityGroup(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Catalogue/ExportCommodityGroup`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportCharge(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Catalogue/ExportCharge`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map(data => data)
        );
    }

    exportStage(searchObject) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Catalogue/ExportStage`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportUnit(searchObject) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Catalogue/ExportUnit`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportCountry(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Catalogue/ExportCountry`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportProvince(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Catalogue/ExportProvince`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportDistrict(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Catalogue/ExportDistrict`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportTownWard(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Catalogue/ExportTownWard`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportSOAOPS(soaNo: string) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/AccountingReport/ExportSOAOPS`, null, { soaNo: soaNo }).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }


    exportBravoSOA(soaNo: string) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/AccountingReport/ExportBravoSOA`, null, { soaNo: soaNo }).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportAdvancePaymentDetail(advanceId: string, language: string) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/${language}/AccountingReport/ExportDetailAdvancePayment?advanceId=${advanceId}&language=${language}`).pipe(
            catchError((error) => throwError(error)),
            map(data => data)
        );
    }

    exportSOAAirFreight(soaNo: string, officeId: string) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/AccountingReport/ExportSOAAirfreight`, null, { soaNo: soaNo, officeId: officeId }).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportSOASupplierAirFreight(soaNo: string, officeId: string) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/AccountingReport/ExportSOASupplierAirfreight`, null, { soaNo: soaNo, officeId: officeId }).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportSettlementPaymentDetail(settlementId: string, language: string) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/${language}/AccountingReport/ExportDetailSettlementPayment?settlementId=${settlementId}&language=${language}`).pipe(
            catchError((error) => throwError(error)),
            map(data => data)
        );
    }

    exportGeneralSettlementPayment(settlementId: string) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/AccountingReport/ExportGeneralSettlementPayment?settlementId=${settlementId}`).pipe(
            catchError((error) => throwError(error)),
            map(data => data)
        );
    }

    exportShipmentOverview(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Documentation/ExportShipmentOverview`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportShipmentOverviewWithType(searchObject: any = {}, reportType: string) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Documentation/ExportShipmentOverviewWithType`, searchObject, { reportType: reportType }).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportDetailSOA(soaNo: string, currency) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/AccountingReport/ExportDetailSOA`, null, { soaNo: soaNo, currency: currency }).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportMawbAirwayBill(jobId: string) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Documentation/ExportMAWBAirExport?jobId=${jobId}`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportHawbAirwayBill(hblid: string, officeId: string) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Documentation/ExportHAWBAirExport?hblid=${hblid}&officeId=${officeId}`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportAdvancePaymentShipment(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/AccountingReport/ExportAdvancePaymentShipment`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportSettlementPaymentShipment(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/AccountingReport/ExportSettlementPaymentShipment`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportAcountingPaymentShipment(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/AccountingReport/ExportAccountingPayment`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportSCSCAirwayBill(jobId: string) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Documentation/ExportSCSCAirExport?jobId=${jobId}`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportTCSAirwayBill(jobId: string) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Documentation/ExportTCSAirExport?jobId=${jobId}`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportACSAirwayBill(jobId: string) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Documentation/ExportACSAirExport?jobId=${jobId}`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportNCTSALSAirwayBill(jobId: string) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Documentation/ExportNCTSALSAirExport?jobId=${jobId}`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportAccountingPLSheet(body: any) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Documentation/ExportAccountingPlSheet`, body).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportJobProfitAnalysis(body: any) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Documentation/ExportJobProfitAnalysis`, body).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportStandardGeneralReport(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Documentation/ExportStandardGeneralReport`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportChartOfAccounts(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Catalogue/ExportChartOfAccounts`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map(data => data)
        );
    }

    exportSummaryOfCostsIncurred(body: any) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Documentation/exportSummaryOfCostsIncurred`, body).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportSummaryOfRevenueIncurred(body: any) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Documentation/exportSummaryOfRevenueIncurred`, body).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportCostsByPartner(body: any) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Documentation/exportSummaryOfCostsPartner`, body).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportAccountingManagement(body: any) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/AccountingReport/ExportAccountingManagement`, body).pipe(
            catchError((error) => throwError(error)),
            map(data => data)
        );
    }

    exportHousebillDaily(issuedDate: string) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Documentation/ExportHousebillDaily?issuedDate=${issuedDate}`).pipe(
            catchError((error) => throwError(error)),
            map(data => data)
        );
    }

    exportCDNote(jobId: string, cdNo: String, officeId: string) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/en-US/Documentation/ExportOpsCdNote`, null, { jobId: jobId, cdNo: cdNo, officeId: officeId }).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportCommissionPRReport(searchObject: any = {}, currentUserId: string, rptType: string) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Documentation/ExportCommissionPRReport`, searchObject, { currentUserId: currentUserId, rptType: rptType }).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportIncentiveReport(searchObject: any = {}, currentUserId: string) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Documentation/ExportIncentiveReport`, searchObject, { currentUserId: currentUserId });
    }

    exportCDNoteCombine(data: any[]) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/en-US/Documentation/ExportOpsCdNoteCombine`, data).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportUnlockRequest(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/SettingReport/ExportUnlockRequest`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map(data => data)
        );
    }

    previewExportPayment(id: string, language: string,moduleName:string) {
        if (moduleName ==='Settlement') 
            window.open(`https://view.officeapps.live.com/op/view.aspx?src=${environment.HOST.EXPORT}/api/v1/${language}/AccountingReport/ExportDetailSettlementPayment?settlementId=${id}&language=${language}`, '_blank');
        else if (moduleName ==='Advance') 
            window.open(`https://view.officeapps.live.com/op/view.aspx?src=${environment.HOST.EXPORT}/api/v1/${language}/AccountingReport/ExportDetailAdvancePayment?advanceId=${id}&language=${language}`, '_blank');
    }
}

