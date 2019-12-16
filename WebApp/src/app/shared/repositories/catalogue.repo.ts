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

    addCurrency(data: any) {
        return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCurrency/Add`, data).pipe(
            catchError((error) => throwError(error)),
            map((res: any) => {
                return res;
            })
        );
    }

    updateCurrency(data: any) {
        return this._api.put(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCurrency/Update`, data).pipe(
            catchError((error) => throwError(error)),
            map((res: any) => {
                return res;
            })
        );
    }

    deleteCurrency(id: string) {
        return this._api.delete(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/vi/CatCurrency/${id}`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
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


    getListCurrency(page?: number, size?: number, body?: any) {
        if (!!page && !!size && !!body) {
            return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCurrency/paging`, body, {
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
    getProvincesBycountry(id: any) {
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatPlace/GetProvinces`, { countryId: id }).pipe(
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
    getDistrictsByProvince(id: any) {
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatPlace/GetDistricts`, { provinceId: id }).pipe(
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
    getAreas() {
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatArea`).pipe(
            map((res: any) => {
                return res;
            })
        );
    }
    getModeOfTransport() {
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatPlace/GetModeOfTransport`).pipe(
            map((res: any) => {
                return res;
            })
        );
    }
    getCharges(body?: any) {
        return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCharge/Query`, body).pipe(
            map((res: any) => {
                return res;
            })
        );
    }

    getDetailPartner(id: string) {
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatPartner/${id}`).pipe(
            map((data: any) => data)
        );
    }

    createPartner(body: any = {}) {
        return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/vi/CatPartner/Add`, body).pipe(
            map((data: any) => data)
        );
    }

    updatePartner(id: string, body: any = {}) {
        return this._api.put(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/vi/CatPartner/` + id, body).pipe(
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
    downloadPlaceExcel(placeType: any) {
        return this._api.downloadfile(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/vi/CatPlace/DownloadExcel`, null, { type: placeType }).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    upLoadPlaceFile(files: any, placeType: any) {
        return this._api.postFile(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatPlace/UploadFile`, files, "uploadedFile", { type: placeType });
    }
    importPlace(body: any) {
        return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/vi/CatPlace/Import`, body).pipe(
            map((data: any) => data)
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

    getCommodityPaging(page?: number, size?: number, body: any = {}) {
        return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCommonity/paging`, body, {
            page: '' + page,
            size: '' + size
        }).pipe(
            map((data: any) => data)
        );
    }

    getCommodityGroupPaging(page?: number, size?: number, body: any = {}) {
        return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCommodityGroup/paging`, body, {
            page: '' + page,
            size: '' + size
        }).pipe(
            map((data: any) => data)
        );
    }

    getAllCommodityGroup() {
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCommodityGroup/GetByLanguage`)
            .pipe(
                map((data: any) => data)
            );
    }

    deleteCommodity(id: number) {
        return this._api.delete(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCommonity/${id}`).pipe(
            map((data: any) => data)
        );
    }

    deleteCommodityGroup(id: number) {
        return this._api.delete(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCommodityGroup/${id}`).pipe(
            map((data: any) => data)
        );
    }

    addNewCommodity(body: any) {
        return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCommonity/Add`, body).pipe(
            map((data: any) => data)
        );
    }

    updateCommodity(id: number, body: any) {
        return this._api.put(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCommonity/${id}`, body).pipe(
            map((data: any) => data)
        );
    }

    addNewCommodityGroup(body: any) {
        return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCommodityGroup/Add`, body).pipe(
            map((data: any) => data)
        );
    }

    updateCommodityGroup(id: number, body: any) {
        return this._api.put(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCommodityGroup/${id}`, body).pipe(
            map((data: any) => data)
        );
    }

    getDetailCommodity(id: number) {
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCommonity/${id}`).pipe(
            map((data: any) => data)
        );
    }

    getDetailCommodityGroup(id: number) {
        return this._api.get(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCommodityGroup/${id}`).pipe(
            map((data: any) => data)
        );
    }

    downloadCommodityExcel() {
        return this._api.downloadfile(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/vi/CatCommonity/downloadExcel`, null).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    upLoadCommodityFile(files: any) {
        return this._api.postFile(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCommonity/uploadFile`, files, "uploadedFile");
    }

    importCommodity(body: any) {
        return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/vi/CatCommonity/import`, body).pipe(
            map((data: any) => data)
        );
    }

    downloadCommodityGroupExcel() {
        return this._api.downloadfile(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/vi/CatCommodityGroup/downloadExcel`, null).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    upLoadCommodityGroupFile(files: any) {
        return this._api.postFile(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCommodityGroup/uploadFile`, files, "uploadedFile");
    }

    importCommodityGroup(body: any) {
        return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/vi/CatCommodityGroup/import`, body).pipe(
            map((data: any) => data)
        );
    }

    deleteCharge(id: string) {
        return this._api.delete(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/vi/CatCharge/delete/${id}`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    addCharge(data: any) {
        return this._api.post(`${environment.HOST.CATALOGUE}/api/${this.VERSION}/en-US/CatCharge/addNew`, data).pipe(
            catchError((error) => throwError(error)),
            map((res: any) => {
                return res;
            })
        );
    }



}
