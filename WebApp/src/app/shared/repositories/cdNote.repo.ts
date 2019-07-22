import { Injectable } from "@angular/core";
import { ApiService } from "../services";
import { environment } from "src/environments/environment";

@Injectable()
export class CDNoteRepo {

    private MODULE: string = 'Documentation';
    private VERSION: string = 'v1';
    private baseApi: string = environment.HOST.WEB_URL;
    constructor(protected _api: ApiService) {
        this.baseApi = this._api.getApiUrl(this.baseApi, 44366, this.MODULE);
        // if (this.baseApi.includes('localhost')) {
        //     this.baseApi = `${this.baseApi}44366`;
        // } else {
        //     this.baseApi = `${this.baseApi}/${this.MODULE}`;
        // }
    }

    getListCDNoteByHouseBill(houseBillId: string) {
        return this._api.get(`${this.baseApi}/api/${this.VERSION}/vi/AcctCDNote/Get`, { Id: houseBillId, IsHouseBillID: true });
        // return this._api.get(`${environment.HOST.WEB_URL}/${this.MODULE}/api/${this.VERSION}/vi/AcctCDNote/Get`, { Id: houseBillId, IsHouseBillID: true });
    }
}
