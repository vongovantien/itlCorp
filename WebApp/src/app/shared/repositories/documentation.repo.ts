import { Injectable } from "@angular/core";
import { ApiService } from "../services";
import { environment } from "src/environments/environment";
import { catchError, map } from "rxjs/operators";
import { throwError } from "rxjs";

@Injectable()
export class DocumentationRepo {

    private VERSION: string = 'v1';
    constructor(protected _api: ApiService) {
    }

    createTransaction(body: any) {
        return this._api.post(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/vi/CsTransaction`, body);
    }

    getDetailTransaction(id: string) {
        return this._api.get(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/vi/CsTransaction/${id}`);
    }

    updateCSTransaction(body: any) {
        return this._api.put(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/vi/CsTransaction`, body);
    }

    addOPSJob(body: any = {}) {
        return this._api.post(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/vi/OpsTransaction/Add`, body);
    }

    getListShipment(page?: number, size?: number, body = {}) {
        return this._api.post(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/vi/OpsTransaction/Paging`, body, {
            page: '' + page,
            size: '' + size
        }).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    updateShipment(body: any) {
        return this._api.put(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/vi/OpsTransaction/Update`, body).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    getDetailShipment(id: string) {
        return this._api.get(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/vi/OpsTransaction?id=${id}`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    getOPSShipmentCommonData() {
        return this._api.get(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/vi/Terminology/GetOPSShipmentCommonData`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    checkShipmentAllowToDelete(id: string) {
        return this._api.get(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/vi/OpsTransaction/CheckAllowDelete/${id}`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    deleteShipment(id: string) {
        return this._api.delete(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/vi/OpsTransaction/Delete/${id}`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    getListCDNoteByHouseBill(houseBillId: string) {
        return this._api.get(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/vi/AcctCDNote/Get`, { Id: houseBillId, IsHouseBillID: true });
    }

    getShipmentByPartnerOrService(partnerId: string, services: string[]) {
        return this._api.get(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/en-US/Shipment/GetShipmentsCreditPayer`, { partner: partnerId, productServices: services }).pipe(
            map((data: any) => data)
        );
    }

    getShipmentNotLocked() {
        return this._api.get(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/en-US/Shipment/GetShipmentNotLocked`).pipe(
            map((data: any) => data)
        );
    }

    getListChargeShipment(body: any = {}) {
        return this._api.post(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/vi/CsShipmentSurcharge/ListChargeShipment`, body).pipe(
            map((data: any) => data)
        );
    }

    convertClearanceToJob(body: any) {
        return this._api.post(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/en-US/OpsTransaction/ConvertExistedClearancesToJobs`, body).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    getDetailsCDNote(jobId: string, cdNo: String) {
        return this._api.get(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/en-US/AcctCDNote/GetDetails`, { jobId: jobId, cdNo: cdNo });
    }

    getSurchargeByHbl(type: string, hbId: string) {
        return this._api.get(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/en-US/CsShipmentSurcharge/GetByHB`, { hbId: hbId, type: type });
    }

    getListContainersOfJob(data: any = {}) {
        return this._api.post(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/vi/CsMawbcontainer/Query`, data).pipe(
            catchError((error) => throwError(error)),
            map((res: any) => {
                return res;
            })
        );
    }

    getShipmentCommonData() {
        return this._api.get(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/vi/Terminology/GetOPSShipmentCommonData`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    downloadcontainerfileExcel() {
        return this._api.downloadfile(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/vi/CsMawbcontainer/downloadFileExcel`, null).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    previewCDNote(data) {
        return this._api.post(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/en-US/AcctCDNote/PreviewOpsCdNote`, data).pipe(
            catchError((error) => throwError(error)),
            map((res: any) => {
                return res;
            })
        );
    }

    previewPL(jobId, currency) {
        return this._api.get(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/en-US/OpsTransaction/PreviewFormPLsheet`, { jobId: jobId, currency: currency }).pipe(
            map((data: any) => data)
        );
    }

    importContainerExcel(data) {
        return this._api.post(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/vi/CsMawbcontainer/Import`, data).pipe(
            map((res: any) => {
                return res;
            })
        );
    }

    getShipmentBySearchOption(searchOption: string, keywords: string[] = []) {
        return this._api.get(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/en-US/Shipment/GetShipmentsCopyListBySearchOption`, { searchOption: searchOption, keywords: keywords }).pipe(
            map((data: any) => data)
        );
    }

    getListHourseBill(data: any = {}) {
        return this._api.post(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/vi/CsTransactionDetail/QueryData`, data)
    }

    getListShipmentDocumentation(page?: number, size?: number, body: any = {}) {
        return this._api.post(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/en-US/CsTransaction/Paging`, body, {
            page: '' + page,
            size: '' + size
        }).pipe(
            map((data: any) => data)
        );
    }

    getListHouseBillOfJob(data: any = {}) {
        return this._api.post(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/en-US/CsTransactionDetail/QueryData`, data).pipe(
            catchError((error) => throwError(error)),
            map((res: any) => {
                return res;
            })
        );
    }

    createHousebill(body: any = {}) {
        return this._api.post(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/vi/CsTransactionDetail/addNew`, body)
    }

    checkMasterBillAllowToDelete(id: string) {
        return this._api.get(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/en-US/CsTransaction/CheckAllowDelete/${id}`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    deleteMasterBill(id: string) {
        return this._api.delete(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/en-US/CsTransaction/${id}`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    getShipmentDataCommon() {
        return this._api.get(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/vi/Terminology/GetShipmentCommonData`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    getPartners(id: any){
        return this._api.get(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/en-US/CsShipmentSurcharge/GetPartners`,{ Id: id, IsHouseBillID : true}).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    getChargesByPartner(id: any, partnerId: any){
        return this._api.get(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/en-US/CsShipmentSurcharge/GroupByListHB`,{ Id: id, partnerID: partnerId, IsHouseBillID: true}).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    getListCdNoteByMasterBill(id: string){
        return this._api.get(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/en-US/AcctCDNote/Get`,{ Id: id, IsHouseBillID: false}).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    checkCdNoteAllowToDelete(id: string) {
        return this._api.get(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/en-US/AcctCDNote/CheckAllowDelete/${id}`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    deleteCdNote(cdNoteId: string){
        return this._api.delete(`${environment.HOST.DOCUMENTATION}/api/${this.VERSION}/en-US/AcctCDNote/Delete`,{cdNoteId: cdNoteId}).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

}
