import { Injectable } from "@angular/core";
import { ApiService } from "../services";
import { environment } from "src/environments/environment";
import { throwError } from "rxjs";
import { catchError, map } from "rxjs/operators";

@Injectable()
export class UnitRepo {

    private VERSION: string = 'v1';
    constructor(protected _api: ApiService) {
    }

    getListUnitByType(data: any = {}) {
        // return this._api.post(`${environment.HOST.WEB_URL}/Catalogue/api/${this.VERSION}/vi/CatUnit/Query`, data).pipe(
        //     catchError((error) => throwError(error)),
        //     map((res: any) => {
        //         return res;
        //     })
        // );
        return this._api.post(`${environment.HOST.WEB_URL}44361/api/${this.VERSION}/vi/CatUnit/Query`, data).pipe(
            catchError((error) => throwError(error)),
            map((res: any) => {
                return res;
            })
        );
    }

    getAllUnit() {
        // return this._api.get(`${environment.HOST.WEB_URL}/Catalogue/api/${this.VERSION}/en-US/CatUnit`).pipe(
        //     catchError((error) => throwError(error)),
        //     map((data: any) => {
        //         return data;
        //     })
        // );
        return this._api.get(`${environment.HOST.WEB_URL}44361/api/${this.VERSION}/en-US/CatUnit`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => {
                return data;
            })
        );
    }
}