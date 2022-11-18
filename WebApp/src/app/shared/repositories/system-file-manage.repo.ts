import { Injectable } from "@angular/core";
import { throwError } from "rxjs";
import { catchError, map } from "rxjs/operators";
import { environment } from "src/environments/environment";
import { ApiService } from "../services";

@Injectable({ providedIn: 'root' })
export class SystemFileManageRepo {
    private VERSION: string = 'v1';
    constructor(protected _api: ApiService) {
    }

    uploadFile(moduleName: string, folder: string, id: string, files: any) {
        return this._api.putFile(`${environment.HOST.FILE_SYSTEM}/api/${this.VERSION}/en-US/AWSS3/UploadAttachedFiles/${moduleName}/${folder}/${id}`, files, 'files').pipe(
            map((data: any) => data)
        );
    }

    uploadAttachedFileEdoc(moduleName: string, folder: string, id: string, files: any) {
        return this._api.putFile(`${environment.HOST.FILE_SYSTEM}/api/${this.VERSION}/en-US/EDoc/UploadAttachedFileEdoc/${moduleName}/${folder}/${id}`, files, 'files').pipe(
            map((data: any) => data)
        );
    }

    deleteFile(moduleName: string, folder: string, id: string, file: string) {
        return this._api.delete(`${environment.HOST.FILE_SYSTEM}/api/${this.VERSION}/en-US/AWSS3/DeleteSpecificFile/${moduleName}/${folder}/${id}/${file}`).pipe(
            map((data: any) => data)
        );
    }

    getFile(moduleName: string, folder: string, id: string) {
        return this._api.get(`${environment.HOST.FILE_SYSTEM}/api/${this.VERSION}/en-US/AWSS3/GetAttachedFiles/${moduleName}/${folder}/${id}`).pipe(
            map((data: any) => data)
        );
    }

    dowloadallAttach(body: any) {
        return this._api.downloadfile(`${environment.HOST.FILE_SYSTEM}/api/${this.VERSION}/en-US/AWSS3/DowloadAllFileAttached`, body).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    uploadAttachedFiles(folder: string, id: string, files: FileList[], child?: string) {
        if (!!child) {
            return this._api.putFile(`${environment.HOST.FILE_SYSTEM}/api/${this.VERSION}/en-US/AWSS3/UploadAttachedFiles/Accounting/${folder}/${id}`, files, 'files', { child: child });
        }
        return this._api.putFile(`${environment.HOST.FILE_SYSTEM}/api/${this.VERSION}/en-US/AWSS3/UploadAttachedFiles/Accounting/${folder}/${id}`, files, 'files');
    }

    getAttachedFiles(folder: string, id: string, child?: string) {
        if (!!child) {
            return this._api.get(`${environment.HOST.FILE_SYSTEM}/api/${this.VERSION}/en-Us/AWSS3/GetAttachedFiles/Accounting/${folder}/${id}`, { child: child });
        }
        return this._api.get(`${environment.HOST.FILE_SYSTEM}/api/${this.VERSION}/en-Us/AWSS3/GetAttachedFiles/Accounting/${folder}/${id}`);

    }

    deleteAttachedFile(folder: string, id: string) {
        return this._api.delete(`${environment.HOST.FILE_SYSTEM}/api/${this.VERSION}/en-Us/AWSS3/DeleteAttachedFile/Accounting/${folder}/${id}`);
    }

    getDocumentType(transactionType: string, billingId: string) {
        return this._api.get(`${environment.HOST.FILE_SYSTEM}/api/${this.VERSION}/en-US/AttachFileTemplate/GetDocumentType?transactionType=${transactionType}&billingId=${billingId}`).pipe(
            map((data: any) => data)
        );
    }

    uploadEDoc(body: any, files: any, type: string) {
        return this._api.putEDocFile(`${environment.HOST.FILE_SYSTEM}/api/${this.VERSION}/en-US/EDoc/UploadEdoc?type=${type}`, body, files).pipe(
            map((data: any) => data)
        );
    }

    getEDocByJob(jobId: string, transitionType: string) {
        return this._api.get(`${environment.HOST.FILE_SYSTEM}/api/${this.VERSION}/en-US/EDoc/GetEDocByJob?jobId=${jobId}&transactionType=${transitionType}`).pipe(
            map((data: any) => data)
        );
    }

    getEDocByAccountant(billingId: string, transitionType: string) {
        return this._api.get(`${environment.HOST.FILE_SYSTEM}/api/${this.VERSION}/en-US/EDoc/GetEDocByAccountant?billingId=${billingId}&transactionType=${transitionType}`).pipe(
            map((data: any) => data)
        );
    }

    deleteEdoc(edocId: string) {
        return this._api.delete(`${environment.HOST.FILE_SYSTEM}/api/${this.VERSION}/en-US/EDoc/DeleteEDoc/${edocId}`).pipe(
            map((data: any) => data)
        );
    }

    updateEdoc(body: any = {}) {
        return this._api.put(`${environment.HOST.FILE_SYSTEM}/api/${this.VERSION}/en-US/EDoc/UpdateEdoc`, body)
            .pipe(
                map((data: any) => data)
            );
    }

    uploadPreviewTemplateEdoc(body) {
        return this._api.post(`${environment.HOST.FILE_SYSTEM}/api/${this.VERSION}/en-US/EDoc/UploadPreviewTemplateToEDoc`, body);
    }

    downloadEdoc(edocUrl: string) {
        return this._api.downloadEdocFile(edocUrl);
    }

    getFileEdoc(id: string) {
        return this._api.downloadEdocFile(`${environment.HOST.FILE_SYSTEM}/api/${this.VERSION}/en-US/EDoc/OpenFile/${id}`)
    }
}
