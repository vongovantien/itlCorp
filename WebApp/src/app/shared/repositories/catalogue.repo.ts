import { Injectable } from "@angular/core";
import { ApiService } from "../services";
import { environment } from "src/environments/environment";
import { throwError } from "rxjs";
import { catchError, map } from "rxjs/operators";

@Injectable()
export class CatalogueRepo {

    private VERSION: string = 'v1';
    constructor(protected _api: ApiService) {
    }

    getCurrency() {
        // return this._api.get(`${environment.HOST.WEB_URL}/Catalogue/api/${this.VERSION}/en-US/CatCurrency/getAll`).pipe(
        //     catchError((error) => throwError(error)),
        //     map((data: any) => {
        //         return data;
        //     })
        // );
        return this._api.get(`localhost:44361/api/${this.VERSION}/en-US/CatCurrency/getAll`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => {
                return data;
            })
        );
    }

    getCommondity() {
        return this._api.post(`${environment.HOST.WEB_URL}/Catalogue/api/${this.VERSION}/en-US/CatCommonity/Query`, {}).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => {
                return data;
            })
        );
    }

    getCommodityGroup() {
        return this._api.post(`${environment.HOST.WEB_URL}/Catalogue/api/${this.VERSION}/en-US/CatCommodityGroup/Query`, {}).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => {
                return data;
            })
        );
    }
}

    getUnit() {
        return this._api.get(`localhost:44361/api/${this.VERSION}/en-US/CatUnit`).pipe(
            map((res: any) => {
                return res;
            })
        );
    }

    // getUnit(data?: any) {
    //     return this._api.post(`${environment.HOST.WEB_URL}/Catalogue/api/${this.VERSION}/en-US/CatUnit/Query`, data).pipe(
    //         catchError((error) => throwError(error)),
    //         map((data: any) => {
    //             return data;
    //         })
    //     );
    // }
}