import { Injectable } from "@angular/core";
import { ApiService } from "../services";
import { environment } from "src/environments/environment";
import { catchError, map } from "rxjs/operators";
import { throwError } from "rxjs";

@Injectable()
export class AccoutingRepo {

    private VERSION: string = 'v1';
    constructor(protected _api: ApiService) {
    }

    getListChargeShipment(data: any = {}) {
        return this._api.post(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/vi/CsShipmentSurcharge/ListChargeShipment`, data).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => {
                return data;
            })
        );
    }

    createSOA(data: any = {}) {
        return this._api.post(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/vi/AcctSOA/Add`, data).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => {
                return data;
            })
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
                map((data: any) => {
                    return data;
                })
            );
        }
    }

    deleteSOA(soaNO: string) {
        return this._api.delete(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/en-US/AcctSOA/delete`, { soaNo: soaNO })
            .pipe(
                catchError((error) => throwError(error)),
                map((data: any) => {
                    return data;
                })
            );
    }

    getDetaiLSOA(soaNO: string, currency: string) {
        return this._api.get(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/en-US/AcctSOA/GetBySoaNo/${soaNO}&${currency}`)
            .pipe(
                catchError((error) => throwError(error)),
                map((data: any) => {
                    return data;
                })
            );
    }
}
