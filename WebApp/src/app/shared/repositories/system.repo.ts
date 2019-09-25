import { Injectable } from '@angular/core';
import { ApiService } from '../services';
import { environment } from 'src/environments/environment';
import { catchError, map } from 'rxjs/operators';
import { throwError } from 'rxjs';

@Injectable()
export class SystemRepo {
    private VERSION: string = 'v1';
    private MODULE: string = 'System';
    constructor(private _api: ApiService) {
    }

    getListSystemUser() {
        return this._api.get(`${environment.HOST.SYSTEM}/${this.MODULE}/api/${this.VERSION}/vi/SysUser`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => {
                return data;
            })
        );
    }

   

}

