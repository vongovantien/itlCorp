import { Injectable } from "@angular/core";
import { ApiService } from "../services";
import { environment } from "src/environments/environment";
import { catchError, map } from "rxjs/operators";
import { throwError } from "rxjs";

@Injectable()
export class CDNoteRepo {

    private MODULE: string = 'Documentation';
    private VERSION: string = 'v1';
    constructor(protected _api: ApiService) {
    }

    getListCDNoteByHouseBill(houseBillId: string) {
        return this._api.get(`${environment.HOST.WEB_URL}/${this.MODULE}/api/${this.VERSION}/vi/AcctCDNote/Get`, { Id: houseBillId, IsHouseBillID: true });
    }
    getDetails(jobId: string, cdNo: String) {
        return this._api.get(`${environment.HOST.WEB_URL}/${this.MODULE}/api/${this.VERSION}/en-US/AcctCDNote/GetDetails`, { jobId: jobId, cdNo: cdNo });
    }

    getListCustomDeclaration(page: number, pageSize: number, body: any) {
        return this._api.post(`${environment.HOST.WEB_URL}/operation/api/${this.VERSION}/en-US/CustomsDeclaration/Paging`, body, {
            pageNumber: '' + page,
            pageSize: '' + pageSize
        }).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    importCustomClearanceFromEcus() {
        return this._api.post(`${environment.HOST.WEB_URL}/operation/api/${this.VERSION}/en-US/CustomsDeclaration/ImportClearancesFromEcus`, {}).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    convertClearanceToJob(body: any) {
        return this._api.post(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/en-US/OpsTransaction/ConvertExistedClearancesToJobs`, body).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    deleteMultipleClearance(body: any[] = []) {
        return this._api.put(`${environment.HOST.WEB_URL}/operation/api/${this.VERSION}/en-US/CustomsDeclaration/DeleteMultiple`, body).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }
}
