import { Injectable } from '@angular/core';
import { ApiService } from '../services';
import { environment } from 'src/environments/environment';

@Injectable()
export class SystemRepo {
    private VERSION: string = 'v1';
    private MODULE: string = 'System';
    private baseApi: string = environment.HOST.WEB_URL;
    constructor(private _api: ApiService) {
        this.baseApi = this._api.getApiUrl(this.baseApi, 44360, this.MODULE);
        // if (this.baseApi.includes('localhost')) {
        //     this.baseApi = `${this.baseApi}44360`;
        // } else {
        //     this.baseApi = `${this.baseApi}/${this.MODULE}`;
        // }
    }

    getListSystemUser() {
        return this._api.get(`${this.baseApi}/api/${this.VERSION}/vi/SysUser`);
        // return this._api.get(`${environment.HOST.WEB_URL}/${this.MODULE}/api/${this.VERSION}/vi/SysUser`);
    }

    getListCurrency(page?: number, size?: number) {
        if(!!page && !!size) {
            return this._api.post(`${environment.HOST.WEB_URL}/Catalogue/api/${this.VERSION}/en-US/CatCurrency/paging`, {}, {
                page: '' + page,
                size: '' + size
            });
        } else {
            return this._api.get(`${environment.HOST.WEB_URL}/Catalogue/api/${this.VERSION}/en-US/CatCurrency/getAll`);
        }
        
    }

    getListPartner(page?: number, size?: number, data?: any) {
        if (!!page && !!size) {
            return this._api.post(`${environment.HOST.WEB_URL}/Catalogue/api/${this.VERSION}/en-US/CatPartner/paging`, {}, {
                page: '' + page,
                size: '' + size
            });
        } else {
            return this._api.post(`${environment.HOST.WEB_URL}/Catalogue/api/${this.VERSION}/en-US/CatPartner/Query`, data);

        }

    }
}