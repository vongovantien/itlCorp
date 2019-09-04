import { Injectable } from "@angular/core";
import { ApiService } from "../services";
import { environment } from "src/environments/environment";

@Injectable()
export class CustomDeclarationRepo {

    private MODULE: string = 'Operation';
    private VERSION: string = 'v1';
    constructor(protected _api: ApiService) {
    }

    getListNotImportToJob(isImported: boolean) {
        return this._api.post(`${environment.HOST.WEB_URL}/${this.MODULE}/api/${this.VERSION}/vi/CustomsDeclaration/Query`, { "imPorted": isImported });
    }
    getListImportedInJob(jobNo: string) {
        return this._api.get(`${environment.HOST.WEB_URL}/${this.MODULE}/api/${this.VERSION}/vi/CustomsDeclaration/GetBy?jobNo=` + jobNo);
    }
}
