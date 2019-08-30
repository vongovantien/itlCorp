import { Injectable } from "@angular/core";
import { ApiService } from "../services";
import { environment } from "src/environments/environment";
import { throwError } from "rxjs";
import { catchError, map } from "rxjs/operators";

@Injectable()
export class OperationRepo {

    private VERSION: string = 'v1';
    constructor(protected _api: ApiService) {
    }

    getListContainersOfJob(data: any = {}) {
        return this._api.post(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/vi/CsMawbcontainer/Query`, data).pipe(
            catchError((error) => throwError(error)),
            map((res: any) => {
                return res;
            })
        );

    }

    getListShipment(page?: number, size?: number, body = {}) {
        return this._api.post(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/vi/OpsTransaction/Paging`, body, {
            page: '' + page,
            size: '' + size
        }).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    checkShipmentAllowToDelete(id: string) {
        return this._api.get(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/vi/OpsTransaction/CheckAllowDelete/${id}`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    deleteShipment(id: string) {
        return this._api.delete(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/vi/OpsTransaction/Delete/${id}`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    getCustomDeclaration(jobNo: string) {
        return this._api.get(`${environment.HOST.WEB_URL}/Operation/api/${this.VERSION}/vi/CustomsDeclaration/GetBy`, { jobNo: jobNo }).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    getShipmentCommonData() {
        return this._api.get(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/vi/Terminology/GetOPSShipmentCommonData`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }
    downloadcontainerfileExel(saveAsFileName: string) {
        return this._api.get(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/vi/OpsTransaction/DownloadContainerExcel`, { responseType: 'blob' }).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => saveAs(data, saveAsFileName))
        );
    }
    // uploadContainerExcelFile(data) {
    //     return this._api.post(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/vi/OpsTransaction/Paging`, data, ).pipe(
    //         catchError((error) => throwError(error)),
    //         map((data: any) => data)
    //     );
    // }
}


