import { Injectable } from '@angular/core';
import { ApiService } from '../services';
import { environment } from 'src/environments/environment';
import { map, catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { HttpHeaders } from '@angular/common/http';

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
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/SystemReport/ExportCompany`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map(data => data)
        );
    }

    exportDepartment(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/SystemReport/ExportDepartment`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map(data => data)
        );
    }
    exportOffice(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/SystemReport/ExportOffice`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map(data => data)
        );
    }
    exportGroup(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/SystemReport/ExportGroup`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map(data => data)
        );
    }

    exportUser(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/SystemReport/ExportUser`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map(data => data)
        );
    }
    exportEManifest(hblId: string) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Documentation/ExportEManifest`, null, { hblid: hblId }).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportGoodDeclare(hblId: string) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Documentation/ExportGoodsDeclare`, null, { hblid: hblId }).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }
    exportDangerousGoods(hblId: string) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Documentation/ExportDangerousGoods`, null, { hblid: hblId }).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportPartner(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Catalogue/ExportPartnerData`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportCurrency(searchObject) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Catalogue/ExportCurrency`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportPortIndex(searchObject: any = {}) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Catalogue/ExportPortIndex`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportCommodity(searchObject) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Catalogue/ExportCommodityList`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportCommodityGroup(searchObject) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Catalogue/ExportCommodityGroup`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportStage(searchObject) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Catalogue/ExportStage`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    exportUnit(searchObject) {
        return this._api.downloadfile(`${environment.HOST.EXPORT}/api/v1/vi/Catalogue/ExportUnit`, searchObject).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }
}

