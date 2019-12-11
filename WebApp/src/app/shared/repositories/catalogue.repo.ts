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

    getCommondity(body?: any) {
        return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCommonity/Query`, body).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => {
                return data;
            })
        );
    }

    getCommodityGroup(body?: any) {
        return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCommodityGroup/Query`, body).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => {
                return data;
            })
        );
    }


    getUnit(body?: any) {
        if (!!body) {
            return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatUnit/Query`, body).pipe(
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
    getPartnersByType(type, active: boolean = true) {
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatPartner/GetBy`, { partnerGroup: type, active: active }).pipe(
            catchError((error) => throwError(error)),
            map((res: any) => {
                return res;
            })
        );
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

    getListCharge(page?: number, size?: number, body: any = {}) {
        if (!!page && !!size) {
            return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCharge/paging`, body, {
                pageNumber: '' + page,
                pageSize: '' + size
            }).pipe(
                catchError((error) => throwError(error)),
                map((res: any) => {
                    return res;
                })
            );
        } else {
            return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCharge/Query`, body).pipe(
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


    checkTaxCode(taxcode: string) {
        // const header: HttpHeaders = new HttpHeaders();
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatPartner/CheckTaxCode`, { taxcode: taxcode })
            .pipe(
                map((data: any) => data)
            );
        // return this._httpClient.get(`${environment.HOST.CatalogueLocal}/api/${this.VERSION}/en-US/CatSaleMan/GetBy`, { params: { partnerId: partnerId } }).pipe(
        //     catchError((error) => throwError(error)),
        //     map((data: any) => data)
        // );
    }

    getSettlePaymentCharges(keyword: string, size: number = 10) {
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCharge/SettlePaymentCharges`, {
            keySearch: keyword,
            active: true,
            size: size
        }).pipe(
            map((data: any) => data)
        );
    }

    getListPort(body?: any) {
        if (!!body) {
            return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatPlace/Query`, body).pipe(
                map((res: any) => {
                    return res;
                })
            );
        } else {
            this.getPlace();
        }
    }

    getListPortByTran() {
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatPlace/GetByModeTran`).pipe(
            map((res: any) => {
                return res;
            })
        );
    }


    getListAllCountry() {
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCountry/getAll`).pipe(
            map((res: any) => {
                return res;
            })
        );
    }

    getCountryByLanguage() {
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCountry/GetByLanguage`).pipe(
            map((res: any) => {
                return res;
            })
        );
    }

    getProvinces() {
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatPlace/GetProvinces`).pipe(
            map((res: any) => {
                return res;
            })
        );
    }

    getAllProvinces() {
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatPlace/GetAllProvinces`).pipe(
            map((res: any) => {
                return res;
            })
        );
    }


    getDistricts() {
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatPlace/GetDistricts`).pipe(
            map((res: any) => {
                return res;
            })
        );
    }

    getPlace(body?: any) {
        if (!!body) {
            return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatPlace/Query`, body).pipe(
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
    getCharges(body?: any) {
        return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCharge/Query`, body).pipe(
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


    checkExistedSaleman(service: string, office: string) {
        const body = { service: service, office: office };
        return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatSaleMan/CheckExisted`, body).pipe(
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

    getStage(body?: any) {
        if (!!body) {
            return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatStage/Query`, body).pipe(
                map((res: any) => {
                    return res;
                })
            );
        } else {
            return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatStage/GetAll`).pipe(
                map((res: any) => {
                    return res;
                })
            );
        }

    }

    //#region place
    addPlace(body: any = {}) {
        return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/vi/CatPlace/Add`, body).pipe(
            map((data: any) => data)
        );
    }
    deletePlace(id: string) {
        return this._api.delete(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/vi/CatPlace/${id}`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }
    updatePlace(id: string, body: any = {}) {
        return this._api.put(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/vi/CatPlace/` + id, body).pipe(
            map((data: any) => data)
        );
    }
    getDetailPlace(id: string) {
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatPlace/` + id)
            .pipe(
                map((data: any) => data)
            );
    }
    pagingPlace(page: number, size: number, body: any = {}) {
        return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatPlace/paging`, body, {
            page: '' + page,
            size: '' + size
        }).pipe(
            catchError((error) => throwError(error)),
            map((res: any) => {
                return res;
            })
        );
    }
    //#endregion

    addCountry(body: any = {}) {
        return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/vi/CatCountry/addNew`, body).pipe(
            map((data: any) => data)
        );
    }
    updateCountry(body: any = {}) {
        return this._api.put(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/vi/CatCountry/update`, body).pipe(
            map((data: any) => data)
        );
    }

    pagingCountry(page: number, size: number, body: any = {}) {
        return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCountry/paging`, body, {
            page: '' + page,
            size: '' + size
        }).pipe(
            catchError((error) => throwError(error)),
            map((res: any) => {
                return res;
            })
        );
    }

    getCountry(body: any = { active: true }) {
        return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/vi/CatCountry/Query`, body).pipe(
            map((data: any) => data)
        );
    }

    getDetailCountry(id: number) {
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCountry/getById/` + id)
            .pipe(
                map((data: any) => data)
            );
    }

    convertExchangeRate(date: string, fromCurrency: string, localCurrency: string = 'VND') {
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCurrencyExchange/ConvertRate`, {
            date: date,
            localCurrency: localCurrency,
            fromCurrency: fromCurrency
        })
            .pipe(
                map((data: any) => data)
            );
    }
}
