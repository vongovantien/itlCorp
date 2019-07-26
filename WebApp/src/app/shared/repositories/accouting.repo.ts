import { Injectable } from "@angular/core";
import { ApiService } from "../services";
import { environment } from "src/environments/environment";

@Injectable()
export class AccoutingRepo {

    private VERSION: string = 'v1';
    constructor(protected _api: ApiService) {
    }

    getListChargeShipment(data: any = {}) {
        return this._api.post(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/vi/CsShipmentSurcharge/ListChargeShipment`, data);
    }

    createSOA(data: any = {}) {
        return this._api.post(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/vi/AcctSOA/Add`, data);
    }

    getListSOA(page?: number, size?: number, data: any = {}) {
        if (!!page && !!size) {
            return this._api.post(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/en-US/AcctSOA/paging`, data, {
                pageNumber: '' + page,
                pageSize: '' + size
            });
        } else {
            return this._api.get(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/en-US/AcctSOA`);
        }
    }
}
