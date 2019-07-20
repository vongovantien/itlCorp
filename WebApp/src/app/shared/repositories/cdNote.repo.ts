import { Injectable } from "@angular/core";
import { ApiService } from "../services";
import { environment } from "src/environments/environment";

@Injectable()
export class CDNoteRepo {

    private MODULE: string = 'Documentation';
    private VERSION: string = 'v1';
    constructor(protected _api: ApiService) {
    }

    getListCDNoteByHouseBill(houseBillId: string) {
        return this._api.get(`${environment.HOST.CD_NOTE}/${this.MODULE}/api/${this.VERSION}/vi/AcctCDNote/Get`, { Id: houseBillId, IsHouseBillID: true });
    }
}
