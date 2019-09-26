import { Injectable } from '@angular/core';
import { ApiService } from '../services';
import { catchError, map } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable()
export class ReportRepo {
    constructor(private _api: ApiService) {
    }

    exportCustomClearance(searchObject: any = {}) {
        return this._api.post(`${environment.HOST.REPORT}/api/v1/vi/ReportData/CustomsDeclaration/ExportCustomClearance`, searchObject, {
            responseType: 'arraybuffer'
        })
    }



}

