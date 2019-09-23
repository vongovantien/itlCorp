import { Injectable } from "@angular/core";
import { ApiService } from "../services";
import { environment } from "src/environments/environment";
import { catchError, map } from "rxjs/operators";
import { throwError, Observable } from "rxjs";

@Injectable()
export class AccoutingRepo {

    private VERSION: string = 'v1';
    constructor(protected _api: ApiService) {
    }

    getListChargeShipment(body: any = {}) {
        return this._api.post(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/vi/CsShipmentSurcharge/ListChargeShipment`, body).pipe(
            map((data: any) => data)
        );
    }

    createSOA(body: any = {}) {
        return this._api.post(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/vi/AcctSOA/Add`, body).pipe(
            map((data: any) => data)
        );
    }

    getListSOA(page?: number, size?: number, body: any = {}) {
        if (!!page && !!size) {
            return this._api.post(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctSOA/paging`, body, {
                pageNumber: '' + page,
                pageSize: '' + size
            }).pipe(
                catchError((error) => throwError(error)),
                map((data: any) => data)
            );
        } else {
            return this._api.get(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctSOA`).pipe(
                map((data: any) => data)
            );
        }
    }

    deleteSOA(soaNO: string) {
        return this._api.delete(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctSOA/delete`, { soaNo: soaNO })
            .pipe(
                map((data: any) => data)
            );
    }

    getDetaiLSOA(soaNO: string, currency: string) {
        return this._api.get(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctSOA/GetBySoaNo/${soaNO}&${currency}`)
            .pipe(
                map((data: any) => data)
            );
    }

    updateSOA(body: any = {}) {
        return this._api.put(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctSOA/update`, body)
            .pipe(
                map((data: any) => data)
            );
    }

    getListShipmentAndCDNote(body: any = {}) {
        return this._api.post(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctSOA/GetShipmentsAndCDdNotesNotExistInResultFilter`, body)
            .pipe(
                map((data: any) => data)
            );
    }

    getListMoreCharge(dataSearch: any = {}) {
        return this._api.post(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctSOA/GetListMoreChargeByCondition`, dataSearch)
            .pipe(
                map((data: any) => data)
            );
    }

    addMoreCharge(body: any = {}) {
        return this._api.post(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctSOA/AddMoreCharge`, body)
            .pipe(
                map((data: any) => data)
            );
    }

    getDetailSOAToExport(soaNO: string, currency: string) {
        return this._api.get(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctSOA/GetDataExportSOABySOANo`, { soaNo: soaNO, currencyLocal: currency })
            .pipe(
                map((data: any) => data)
            );
    }

    getListCustomsDeclaration() {
        return this._api.get(`${environment.HOST.WEB_URL}/operation/api/${this.VERSION}/en-US/CustomsDeclaration`)
            .pipe(
                map((data: any) => data)
            );
    }

    getListShipmentDocumentOperation() {
        return this._api.get(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctAdvancePayment/GetShipments`)
            .pipe(
                map((data: any) => data)
            );
    }

    // add new advance payment with payment request
    addNewAdvancePayment(body: any = {}): Observable<any> {
        return this._api.post(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctAdvancePayment/Add`, body)
            .pipe(
                map((data: any) => data)
            );
    }

    updateAdvPayment(body: any = {}): Observable<any> {
        return this._api.put(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctAdvancePayment/Update`, body)
            .pipe(
                map((data: any) => data)
            );
    }

    sendRequestAdvPayment(body: any = {}): Observable<any> {
        return this._api.post(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctAdvancePayment/SaveAndSendRequest`, body)
            .pipe(
                map((data: any) => data)
            );
    }

    checkShipmentsExistInAdvancePament(body: any) {
        return this._api.post(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctAdvancePayment/CheckShipmentsExistInAdvancePament`, body)
            .pipe(
                map((data: any) => data)
            );
    }

    getListAdvancePayment(page?: number, size?: number, body: any = {}) {
        return this._api.post(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctAdvancePayment/paging`, body, {
            pageNumber: '' + page,
            pageSize: '' + size
        }).pipe(
            map((data: any) => data)
        );
    }

    getDetailAdvancePayment(advanceId: string) {
        return this._api.get(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctAdvancePayment/GetAdvancePaymentByAdvanceId`, { advanceId: advanceId }).pipe(
            map((data: any) => data)
        );
    }

    deleteAdvPayment(advanceNo: string) {
        return this._api.delete(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctAdvancePayment/Delete`, { advanceNo: advanceNo }).pipe(
            map((data: any) => data)
        );
    }

    getGroupRequestAdvPayment(advanceNo: string) {
        return this._api.get(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctAdvancePayment/GetGroupRequestsByAdvanceNo`, { advanceNo: advanceNo }).pipe(
            map((data: any) => data)
        );
    }

    previewAdvancePayment(param: any) {
        if (typeof param === 'string') {
            return this._api.post(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctAdvancePayment/PreviewAdvancePaymentRequestByAdvanceId`, null, { advanceId: param }).pipe(
                map((data: any) => data)
            );
        } else {
            return this._api.post(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctAdvancePayment/PreviewAdvancePaymentRequest`, param).pipe(
                map((data: any) => data)
            );
        }
    }

    getInfoApprove(advanceNo: string) {
        return this._api.get(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctAdvancePayment/GetInfoApproveAdvanceByAdvanceNo`, { advanceNo: advanceNo }).pipe(
            map((data: any) => data)
        );
    }

    approveAdvPayment(advanceId: string) {
        return this._api.post(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctAdvancePayment/UpdateApprove`, {}, { advanceId: advanceId })
            .pipe(
                map((data: any) => data)
            );
    }

    deniedApprove(advanceId: string, comment: string) {
        return this._api.post(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctAdvancePayment/DeniedApprove`, {}, { advanceId: advanceId, comment: comment })
            .pipe(
                map((data: any) => data)
            );
    }

    getShipmentNotLocked() {
        return this._api.get(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/en-US/Shipment/GetShipmentNotLocked`).pipe(
            map((data: any) => data)
        );
    }

    getSettlePaymentCharges(keyword: string, size: number = 10) {
        return this._api.get(`${environment.HOST.WEB_URL}/Catalogue/api/${this.VERSION}/en-US/CatCharge/SettlePaymentCharges`, {
            keySearch: keyword,
            inActive: false,
            size: size
        }).pipe(
            map((data: any) => data)
        );
    }

    addShipmentSurCharge(body: any = {}) {
        return this._api.post(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/CsShipmentSurcharge/Add`, body)
            .pipe(
                map((data: any) => data)
            );
    }

    getListSettlementPayment(page?: number, size?: number, body: any = {}) {
        return this._api.post(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctSettlementPayment/paging`, body, {
            pageNumber: '' + page,
            pageSize: '' + size
        }).pipe(
            map((data: any) => data)
        );
    }

    getShipmentOfSettlements(settlementNo: string) {
        return this._api.get(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctSettlementPayment/GetShipmentOfSettlements`, { settlementNo: settlementNo }).pipe(
            map((data: any) => data)
        );
    }

    getShipmentByPartnerOrService(partnerId: string, services: string[]) {
        return this._api.get(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/Shipment/GetShipmentsCreditPayer`, { partner: partnerId, productServices: services }).pipe(
            map((data: any) => data)
        );
    }

    getExistingCharge(jobId: string, hbl: string, mbl: string) {
        return this._api.get(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctSettlementPayment/GetExistsCharge`, { jobId: jobId, HBL: hbl, MBL: mbl }).pipe(
            map((data: any) => data)
        );
    }

    addNewSettlement(body: any = {}) {
        return this._api.post(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctSettlementPayment/Add`, body)
            .pipe(
                map((data: any) => data)
            );
    }

    getDetailSettlementPayment(settlementId: string) {
        return this._api.get(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctSettlementPayment/GetDetailSettlementPaymentById`, { settlementId: settlementId }).pipe(
            map((data: any) => data)
        );
    }

    updateSettlementPayment(body: any = {}) {
        return this._api.put(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctSettlementPayment/Update`, body).pipe(
            map((data: any) => data)
        );
    }

    saveAndSendRequestSettlemntPayment(body: any = {}) {
        return this._api.post(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctSettlementPayment/SaveAndSendRequest`, body).pipe(
            map((data: any) => data)
        );
    }

    deleteSettlement(settlementNo: string) {
        return this._api.delete(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctSettlementPayment/delete`, { settlementNo: settlementNo })
            .pipe(
                map((data: any) => data)
            );
    }

    getPaymentManagement(jobId: string, mbl: string, hbl: string) {
        return this._api.get(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctSettlementPayment/GetPaymentManagementByShipment`, { JobId: jobId, mbl: mbl, hbl: hbl }).pipe(
            map((data: any) => data)
        );
    }

    checkDuplicateShipmentSettlement(body: any = {}) {
        return this._api.post(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctSettlementPayment/CheckDuplicateShipmentSettlement`, body)
            .pipe(
                map((data: any) => data)
            );

    }

    getInfoApproveSettlement(settlementNo: string) {
        return this._api.get(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctSettlementPayment/GetInfoApproveSettlementBySettlementNo`, { settlementNo: settlementNo }).pipe(
            map((data: any) => data)
        );
    }

    approveSettlementPayment(settlementId: string) {
        return this._api.post(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctSettlementPayment/UpdateApprove`, {}, { settlementId: settlementId })
            .pipe(
                map((data: any) => data)
            );
    }

    deniedApproveSettlement(settlementId: string, comment: string) {
        return this._api.post(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctSettlementPayment/DeniedApprove`, {}, { settlementId: settlementId, comment: comment })
            .pipe(
                map((data: any) => data)
            );
    }

    previewSettlementPayment(settlementNo: any) {
        return this._api.post(`${environment.HOST.WEB_URL}/Accounting/api/${this.VERSION}/en-US/AcctSettlementPayment/Preview`, null, { settlementNo: settlementNo }).pipe(
            map((data: any) => data)
        );
    }

}


