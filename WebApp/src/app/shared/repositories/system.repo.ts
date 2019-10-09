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

    getOffice(page?: number, size?: number, body: any = {}) {
        return this._api.post(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysOffice/paging`, body, {
                page: '' + page,
                size: '' + size
            }).pipe(
            map((data: any) => data)
            );
        }


    getListSystemUser() {
        return this._api.get(`${environment.HOST.SYSTEM}/api/${this.VERSION}/vi/SysUser`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => {
                return data;
            })
        );
    }

    getCompany(page?: number, size?: number, body: any = {}) {
        return this._api.post(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysBu/paging`, body, {
            page: '' + page,
            size: '' + size
        }).pipe(
            map((data: any) => data)
        );
    }

    getDetailCompany(id: string) {
        return this._api.get(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysBu/${id}`).pipe(
            map((data: any) => data)
        );
    }

    addNewCompany(body: any) {
        return this._api.post(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysBu/Add`, body).pipe(
            map((data: any) => data)
        );
    }

    getDepartment(page?: number, size?: number, body: any = {}){
        return this._api.post(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/CatDepartment/paging`, body, {
            page: '' + page,
            size: '' + size
        }).pipe(
            map((data: any) => data)
        );
    }

}

