import { Injectable } from "@angular/core";
import { ApiService } from "../services";
import { environment } from "src/environments/environment";
import { throwError } from "rxjs";
import { catchError, map } from "rxjs/operators";
import { HttpClient, HttpHeaders } from "@angular/common/http";

@Injectable()
export class CatalogueRepo {

    private VERSION: string = 'v1';
    constructor(protected _api: ApiService, private _httpClient: HttpClient) {
    }

    getCurrencyBy(data: any = {}) {
        return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCurrency/getAllByQuery`, data).pipe(
            catchError((error) => throwError(error)),
            map((res: any) => {
                return res;
            })
        );
    }

    getCurrency() {
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCurrency/getAll`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => {
                return data;
            })
        );
    }

    getCommondity() {
        return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCommonity/Query`, {}).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => {
                return data;
            })
        );
    }

    getCommodityGroup() {
        return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCommodityGroup/Query`, {}).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => {
                return data;
            })
        );
    }


    getUnit(body?: any) {
        if (!!body) {
            return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatUnit/Query`, { body }).pipe(
                map((res: any) => {
                    return res;
                })
            );
        } else {
            return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatUnit`).pipe(
                map((res: any) => {
                    return res;
                })
            );
        }
    }


    getListCurrency(page?: number, size?: number) {
        if (!!page && !!size) {
            return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCurrency/paging`, {}, {
                page: '' + page,
                size: '' + size
            }).pipe(
                catchError((error) => throwError(error)),
                map((data: any) => {
                    return data;
                })
            );
        } else {
            return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCurrency/getAll`).pipe(
                catchError((error) => throwError(error)),
                map((data: any) => {
                    return data;
                })
            );
        }

    }

    getListPartner(page?: number, size?: number, data?: any) {
        if (!!page && !!size) {
            return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatPartner/paging`, {}, {
                page: '' + page,
                size: '' + size
            }).pipe(
                catchError((error) => throwError(error)),
                map((res: any) => {
                    return res;
                })
            );
        } else {
            return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatPartner/Query`, data).pipe(
                catchError((error) => throwError(error)),
                map((res: any) => {
                    return res;
                })
            );
        }
    }

    getListCharge(page?: number, size?: number) {
        if (!!page && !!size) {
            return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCharge/paging`, {}, {
                page: '' + page,
                size: '' + size
            }).pipe(
                catchError((error) => throwError(error)),
                map((res: any) => {
                    return res;
                })
            );
        } else {
            return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCharge/Query`, {}).pipe(
                catchError((error) => throwError(error)),
                map((res: any) => {
                    return res;
                })
            );
        }
    }

    getListSaleman(partnerId: string) {
        // const header: HttpHeaders = new HttpHeaders();
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatSaleMan/GetBy`, { partnerId: partnerId })
            .pipe(
                map((data: any) => data)
            );
        // return this._httpClient.get(`${environment.HOST.CatalogueLocal}/api/${this.VERSION}/en-US/CatSaleMan/GetBy`, { params: { partnerId: partnerId } }).pipe(
        //     catchError((error) => throwError(error)),
        //     map((data: any) => data)
        // );
    }

    deleteSaleman(id: string) {
        return this._api.delete(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/vi/CatSaleMan/${id}`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }


    getListService() {
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCharge/GetListServices`)
            .pipe(
                catchError((error) => throwError(error)),
                map((data: any) => data)
            );
    }

    getListBranch() {
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatBranch/GetListBranch`)
            .pipe(
                catchError((error) => throwError(error)),
                map((data: any) => data)
            );
    }

    getSettlePaymentCharges(keyword: string, size: number = 10) {
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCharge/SettlePaymentCharges`, {
            keySearch: keyword,
            inActive: false,
            size: size
        }).pipe(
            map((data: any) => data)
        );
    }

    getListPort(body?: any) {
        if (!!body) {
            return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatPlace/Query`, { body }).pipe(
                map((res: any) => {
                    return res;
                })
            );
        } else {
            return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatPlace`).pipe(
                map((res: any) => {
                    return res;
                })
            );
        }
    }

    getListAllCountry() {
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCountry/getAll`).pipe(
            map((res: any) => {
                return res;
            })
        );
    }

    createPartner(body: any = {}) {
        return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/vi/CatPartner/Add`, body).pipe(
            map((data: any) => data)
        );
    }

    createSaleman(body: any = {}) {
        return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/vi/CatSaleMan/Add`, body).pipe(
            map((data: any) => data)
        );
    }

    getListSaleManDetail(page?: number, size?: number, body: any = {}) {
        if (!!page && !!size) {
            return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatSaleMan/paging`, body, {
                page: '' + page,
                size: '' + size
            }).pipe(
                catchError((error) => throwError(error)),
                map((data: any) => data)
            );
        } else {
            return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatSaleMan`).pipe(
                map((data: any) => data)
            );
        }
    }
}
