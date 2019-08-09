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

    getListChargeShipment(data: any = {}) {
        return this._api.post(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/vi/CsShipmentSurcharge/ListChargeShipment`, data).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    createSOA(data: any = {}) {
        return this._api.post(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/vi/AcctSOA/Add`, data).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    getListSOA(page?: number, size?: number, data: any = {}) {
        if (!!page && !!size) {
            return this._api.post(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/en-US/AcctSOA/paging`, data, {
                pageNumber: '' + page,
                pageSize: '' + size
            }).pipe(
                catchError((error) => throwError(error)),
                map((data: any) => data)
            );
        } else {
            return this._api.get(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/en-US/AcctSOA`).pipe(
                catchError((error) => throwError(error)),
                map((data: any) => data)
            );
        }
    }

    deleteSOA(soaNO: string) {
        return this._api.delete(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/en-US/AcctSOA/delete`, { soaNo: soaNO })
            .pipe(
                catchError((error) => throwError(error)),
                map((data: any) => data)
            );
    }

    getDetaiLSOA(soaNO: string, currency: string) {
        return this._api.get(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/en-US/AcctSOA/GetBySoaNo/${soaNO}&${currency}`)
            .pipe(
                catchError((error) => throwError(error)),
                map((data: any) => data)
            );
    }

    updateSOA(data: any = {}) {
        return this._api.put(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/en-US/AcctSOA/update`, data)
            .pipe(
                catchError((error) => throwError(error)),
                map((data: any) => data)
            );
    }

    getListShipmentAndCDNote(data: any = {}) {
        return this._api.post(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/en-US/AcctSOA/GetShipmentsAndCDdNotesNotExistInResultFilter`, data)
            .pipe(
                catchError((error) => throwError(error)),
                map((data: any) => data)
            );
    }

    getListMoreCharge(dataSearch: any = {}) {
        return this._api.post(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/en-US/AcctSOA/GetListMoreChargeByCondition`, dataSearch)
            .pipe(
                catchError((error) => throwError(error)),
                map((data: any) => data)
            );
    }

    addMoreCharge(data: any = {}) {
        return this._api.post(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/en-US/AcctSOA/AddMoreCharge`, data)
            .pipe(
                catchError((error) => throwError(error)),
                map((data: any) => data)
            );
    }

    getDetailSOAToExport(soaNO: string, currency: string) {
        return this._api.get(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/en-US/AcctSOA/GetDataExportSOABySOANo`, { soaNo: soaNO, currencyLocal: currency })
            .pipe(
                catchError((error) => throwError(error)),
                map((data: any) => data)
            );
    }

    getListCustomsDeclaration() {
        return this._api.get(`${environment.HOST.WEB_URL}/operation/api/${this.VERSION}/en-US/CustomsDeclaration`)
            .pipe(
                catchError((error) => throwError(error)),
                map((data: any) => data)
            );
    }

    getListShipmentDocumentOperation() {
        return this._api.get(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/en-US/AcctAdvancePayment/GetShipments`)
            .pipe(
                catchError((error) => throwError(error)),
                map((data: any) => data)
            );
    }

    // add new advance payment with payment request
    addNewAdvancePayment(body: any = {}): Observable<any> {
        return this._api.post(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/en-US/AcctAdvancePayment/Add`, body)
            .pipe(
                catchError((error) => throwError(error)),
                map((data: any) => data)
            );
    }

    checkShipmentsExistInAdvancePament(body: any) {
        return this._api.post(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/en-US/AcctAdvancePayment/CheckShipmentsExistInAdvancePament`, body)
            .pipe(
                catchError((error) => throwError(error)),
                map((data: any) => data)
            );
    }
    


}
