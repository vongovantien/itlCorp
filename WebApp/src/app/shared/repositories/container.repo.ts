import { Injectable } from "@angular/core";
import { ApiService } from "../services";
import { environment } from "src/environments/environment";
import { throwError } from "rxjs";
import { catchError, map } from "rxjs/operators";

@Injectable()
export class ContainerRepo {

    private VERSION: string = 'v1';
    constructor(protected _api: ApiService) {
    }

    getListContainersOfJob(data: any = {}) {
        // return this._api.post(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/vi/CsMawbcontainer/Query`, data).pipe(
        //     catchError((error) => throwError(error)),
        //     map((res: any) => {
        //         return res;
        //     })
        // );
        return this._api.post(`${environment.HOST.WEB_URL}44366/api/${this.VERSION}/vi/CsMawbcontainer/Query`, data).pipe(
            catchError((error) => throwError(error)),
            map((res: any) => {
                return res;
            })
        );
    }
}