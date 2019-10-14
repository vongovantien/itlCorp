import { Injectable } from '@angular/core';
import { ApiService } from '../services';
import { environment } from 'src/environments/environment';
import { map, catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

@Injectable()
export class ExportRepo {
    constructor(private _api: ApiService) {
    }

    exportCustomClearance(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/ReportData/CustomsDeclaration/ExportCustomClearance`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map(data => data)
        );
    }

    exportCompany(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/SystemReport/System/ExportCompany`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map(data => data)
        );
    }



}

