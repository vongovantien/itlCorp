import { Injectable } from '@angular/core';
import { ApiService } from '../services';
import { environment } from 'src/environments/environment';
import { catchError, map } from 'rxjs/operators';
import { throwError } from 'rxjs';

@Injectable()
export class SystemRepo {
    private VERSION: string = 'v1';
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

    deleteOffice(id: string) {
        return this._api.delete(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysOffice/Delete`, { id: id })
            .pipe(
                map((data: any) => data)
            );
    }

    getDetailOffice(id: string) {
        return this._api.get(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysOffice/${id}`).pipe(
            map((data: any) => data)
        );
    }

    updateOffice(body: any = {}) {
        return this._api.put(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysOffice/Update`, body).pipe(
            map((data: any) => data)
        );
    }


    addNewOffice(body: any) {
        return this._api.post(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysOffice/Add`, body).pipe(
            map((data: any) => data)
        );
    }

    getListCompany() {
        return this._api.get(`${environment.HOST.SYSTEM}/api/${this.VERSION}/vi/SysCompany`).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => {
                return data;
            })
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
        return this._api.post(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysCompany/paging`, body, {
            page: '' + page || 1,
            size: '' + size || 10
        }).pipe(
            map((data: any) => data)
        );
    }

    updateCompany(id: string, body: any) {
        return this._api.put(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysCompany/${id}/Update`, body).pipe(
            map((data: any) => data)
        );
    }

    getDetailCompany(id: string) {
        return this._api.get(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysCompany/${id}`).pipe(
            map((data: CommonInterface.IResult) => data.data)
        );
    }

    addNewCompany(body: any) {
        return this._api.post(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysCompany/Add`, body).pipe(
            map((data: any) => data)
        );
    }

    getDepartment(page?: number, size?: number, body: any = {}) {
        return this._api.post(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/CatDepartment/paging`, body, {
            page: '' + page,
            size: '' + size
        }).pipe(
            map((data: any) => data)
        );
    }
    getAllDepartment() {
        return this._api.get(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/CatDepartment`).pipe(
            map((data: any) => data)
        );
    }

    getDetailDepartment(id: number) {
        return this._api.get(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/CatDepartment/${id}`).pipe(
            map((data: any) => data)
        );
    }

    getGroup(page?: number, size?: number, body: any = {}) {
        return this._api.post(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysGroup/paging`, body, {
            page: '' + page,
            size: '' + size
        }).pipe(
            map((data: any) => data)
        );
    }
    getDetailGroup(id: number) {
        return this._api.get(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysGroup/${id}`).pipe(
            map((data: any) => data)
        );
    }
    deleteGroup(id: number) {
        return this._api.delete(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysGroup/${id}`).pipe(
            map((data: any) => data)
        );
    }
    addNewGroup(body: any) {
        return this._api.post(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysGroup`, body).pipe(
            map((data: any) => data)
        );
    }
    updateGroup(body: any) {
        return this._api.put(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysGroup`, body).pipe(
            map((data: any) => data)
        );
    }
    getUsersInGroup(groupId: number) {
        return this._api.get(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysUserGroup/GetByGroup/${groupId}`).pipe(
            map((data: any) => data)
        );
    }
    getUserGroupDetail(id: number) {
        return this._api.get(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysUserGroup/${id}`).pipe(
            map((data: any) => data)
        );
    }
    addUserToGroup(body: any) {
        return this._api.post(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysUserGroup`, body).pipe(
            map((data: any) => data)
        );
    }
    updateUserGroup(body: any) {
        return this._api.put(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysUserGroup`, body).pipe(
            map((data: any) => data)
        );
    }
    deleteUserGroup(id: number) {
        return this._api.delete(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysUserGroup/${id}`).pipe(
            map((data: any) => data)
        );
    }
    getAllOffice() {
        return this._api.get(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysOffice/GetAll`).pipe(
            map((data: any) => data)
        );
    }

    getOfficeByCompany(id: string) {
        return this._api.get(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysOffice/GetByCompany/${id}`).pipe(
            map((data: CommonInterface.IResult) => data.data)
        );
    }

    addNewDepartment(body: any) {
        return this._api.post(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/CatDepartment/Add`, body).pipe(
            map((data: any) => data)
        );
    }

    updateDepartment(body: any) {
        return this._api.put(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/CatDepartment/Update`, body).pipe(
            map((data: any) => data)
        );
    }

    deleteDepartment(id: number) {
        return this._api.delete(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/CatDepartment/Delete`, { id: id }).pipe(
            map((data: any) => data)
        );
    }

    getDepartmentsByOfficeId(id: string) {
        return this._api.get(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/CatDepartment/GetDepartmentByOfficeId`, { id: id }).pipe(
            map((data: any) => data)
        );
    }

    deleteCompany(id: string) {
        return this._api.delete(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysCompany/Delete`, { id: id }).pipe(
            map((data: any) => data)
        );
    }

    getGroupsByDeptId(id: number) {
        return this._api.post(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysGroup/Query`, { departmentId: id }).pipe(
            map((data: any) => data)
        );
    }

    getEmployeeByemployeeid(id: string) {
        return this._api.get(`${environment.HOST.SYSTEM}/api/${this.VERSION}/en-US/SysEmployee/GetByEmployeeId?employeeid=${id}`).pipe(
            map((data: CommonInterface.IResult) => data.data)
        );
    }
}

