import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { SystemConstants } from 'src/constants/system.const';
import { IEDocFile, IEDocUploadFile } from './../../business-modules/share-business/components/document-type-attach/document-type-attach.component';

@Injectable({ providedIn: 'root' })
export class ApiService {

    baseUrl: string = '//';
    private _headers: {} = {};
    constructor(protected _http: HttpClient,
    ) {
    }
    setHeaders(headers: any = {}) {
        this._headers = Object.assign({}, this._headers, headers);
        return this;
    }

    // set resorce to request
    setUrl(url: string = ''): string {
        return `${this.baseUrl}/${url}`;
    };

    getCurrentLanguage() {
        return localStorage.getItem(SystemConstants.CURRENT_LANGUAGE);
    }

    getCurrentVersion() {
        return localStorage.getItem(SystemConstants.CURRENT_VERSION);
    }

    post(url: string, data?: any, params?: any, headers?: any) {
        return this._http
            .post(this.setUrl(url), data, {
                params,
                headers: Object.assign({}, this._headers, headers)
            });
    }

    postFile(url: string, files: any, name: string = null, params?: any) {
        if (files.length === 0) {
            return;
        }
        const formData = new FormData();
        for (const file of files) {
            formData.append(name || file.name, file);
        }
        // const params = new HttpParams();
        const options = {
            params: params,
            reportProgress: true,
            headers: new HttpHeaders({
                'accept': 'application/json'
            })
        };
        return this._http.post(this.setUrl(url), formData, options);
    }

    putFile(url: string, files: any, name: string = null, params?: any) {
        if (files.length === 0) {
            return;
        }
        const formData = new FormData();
        for (const file of files) {
            formData.append(name, file);
        }
        const options = {
            params: params,
            reportProgress: true,
            headers: new HttpHeaders({
                'accept': 'application/json'
            })
        };
        return this._http.put(this.setUrl(url), formData, options);
    }

    postFormData(url: string, formData: any) {
        const options = {
            headers: new HttpHeaders({
                'Content-Type': 'application/x-www-form-urlencoded',
            })
        };
        return this._http.post(this.setUrl(url), formData);

    }

    put(url: string, data?: any, params?: any, headers?: any) {
        return this._http
            .put(this.setUrl(url), data, {
                params,
                headers: Object.assign({}, this._headers, headers)
            });
    }

    downloadfile(url: string, data?: any, params?: any, headers?: any, observe: 'body' | 'response' | any = 'body') {
        if (data !== null && data !== undefined) {
            return this._http.post(this.setUrl(url), data, {
                params,
                headers: Object.assign({}, this._headers, headers),
                responseType: 'arraybuffer',
                observe: observe
            });
        } else {
            return this._http.get(this.setUrl(url), {
                params,
                headers: Object.assign({}, this._headers, headers),
                responseType: 'arraybuffer',
                observe: observe
            });
        }
    }

    get(url: string = '', params?: any, headers: any = {}, isBaseHref: boolean = true) {
        return this._http.get(isBaseHref ? this.setUrl(url) : url, {
            params,
            headers: Object.assign({}, this._headers, headers)
        });
    }

    delete(url: string, params?: any, headers?: any) {
        return this._http.delete(this.setUrl(url), { params, headers });
    }

    putEDocFile(url: string, edoc: IEDocUploadFile, files: any) {
        if (edoc.EDocFiles.length === 0) {
            return;
        }
        const formData = new FormData();
        formData.append('edocUploadModel.ModuleName', edoc.ModuleName);
        formData.append('edocUploadModel.FolderName', edoc.FolderName);
        formData.append('edocUploadModel.Id', edoc.Id);
        let edocFile: IEDocFile[] = [];
        for (let i = 0; i < edoc.EDocFiles.length; i++) {
            let edocFileItem: IEDocFile = ({
                AliasName: edoc.EDocFiles[i].AliasName,
                BillingNo: edoc.EDocFiles[i].BillingNo,
                BillingType: edoc.EDocFiles[i].BillingType,
                Code: edoc.EDocFiles[i].Code,
                //FileInput: edoc.EDocFiles[i].FileInput,
                HBL: edoc.EDocFiles[i].HBL,
                JobId: edoc.EDocFiles[i].JobId,
                TransactionType: edoc.EDocFiles[i].TransactionType,
                FileName: edoc.EDocFiles[i].FileName,
                Note: edoc.EDocFiles[i].Note,
                BillingId: edoc.EDocFiles[i].BillingId
            });
            edocFile.push(edocFileItem);
        }
        console.log(edocFile);
        for (let i = 0; i < edocFile.length; i++) {
            formData.append(`edocUploadModel.EDocFiles[${i}][AliasName]`, edocFile[i].AliasName);
            formData.append(`edocUploadModel.EDocFiles[${i}][BillingNo]`, edocFile[i].BillingNo);
            formData.append(`edocUploadModel.EDocFiles[${i}][BillingType]`, edocFile[i].BillingType);
            formData.append(`edocUploadModel.EDocFiles[${i}][Code]`, edocFile[i].Code);
            //formData.append(`edocUploadModel.EDocFiles[${i}].[File]`, edocFile[i].FileInput);
            formData.append(`edocUploadModel.EDocFiles[${i}][HBL]`, edocFile[i].HBL);
            formData.append(`edocUploadModel.EDocFiles[${i}][JobId]`, edocFile[i].JobId);
            formData.append(`edocUploadModel.EDocFiles[${i}][TransactionType]`, edocFile[i].TransactionType);
            formData.append(`edocUploadModel.EDocFiles[${i}][FileName]`, edocFile[i].FileName);
            formData.append(`edocUploadModel.EDocFiles[${i}][Note]`, edocFile[i].Note);
            formData.append(`edocUploadModel.EDocFiles[${i}][BillingId]`, edocFile[i].BillingId);
        }
        for (const file of files) {
            formData.append('files', file);
        }
        const options = {
            params: null,
            reportProgress: true,
            headers: new HttpHeaders({
                'accept': 'application/json'
            })
        };
        return this._http.put(this.setUrl(url), formData, options);
    }
}