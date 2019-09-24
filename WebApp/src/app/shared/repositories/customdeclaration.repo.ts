import { Injectable } from "@angular/core";
import { ApiService } from "../services";
import { environment } from "src/environments/environment";
import { map } from "rxjs/operators";

@Injectable()
export class CustomDeclarationRepo {

    private MODULE: string = 'Operation';
    private VERSION: string = 'v1';
    constructor(protected _api: ApiService) {
    }

    getListNotImportToJob(strKeySearch: string, customerNo: string, isImported: boolean, page: number, size: number) {
        // return this._api.post(`${environment.HOST.WEB_URL}/${this.MODULE}/api/${this.VERSION}/vi/CustomsDeclaration/Query`, { "imPorted": isImported });
        // return this._api.post(`localhost:44365/api/${this.VERSION}/vi/CustomsDeclaration/Query`, { "imPorted": isImported });
        //return this._api.post(`localhost:44365/api/${this.VERSION}/vi/CustomDeclaration?customNo=` + customNo + '&customerNo=' + customerNo + '&imporTed=' + isImported + '&page=' + page + '&size='+ size);
      //  return this._api.get(`localhost:44365/api/${this.VERSION}/vi/CustomsDeclaration/CustomDeclaration`, { keySearch: strKeySearch, customerNo: customerNo, imporTed: isImported, page: page, size: size }).pipe(
        //    map((data: any) => data)
       // );;
       return this._api.get(`${environment.HOST.WEB_URL}/${this.MODULE}/api/${this.VERSION}/vi/CustomsDeclaration/CustomDeclaration`, { keySearch: strKeySearch, customerNo: customerNo, imporTed: isImported, page: page, size: size }).pipe(
           map((data: any) => data)
        );;
       

    }
    getListImportedInJob(jobNo: string) {
        return this._api.get(`${environment.HOST.WEB_URL}/${this.MODULE}/api/${this.VERSION}/vi/CustomsDeclaration/GetBy?jobNo=` + jobNo);
    }
}
