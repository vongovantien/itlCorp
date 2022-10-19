import { Injectable } from "@angular/core";
import { ApiService } from "../services";
import { environment } from "src/environments/environment";
import { catchError, map } from "rxjs/operators";
import { throwError } from "rxjs";

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
        return this._api.downloadfile(`${environment.HOST.FILE_SYSTEM}/api/${this.VERSION}/en-US/EDoc/DowloadAllFileAttached`, body).pipe(
            catchError((error) => throwError(error)),
            map((data: any) => data)
        );
    }

    getDocumentType(transactionType: string) {
        return this._api.get(`${environment.HOST.FILE_SYSTEM}/api/${this.VERSION}/en-US/EDoc/GetDocumentType?transactionType=${transactionType}`).pipe(
            map((data: any) => data)
        );
    }

    uploadEDoc(body: any, files: any) {
        return this._api.putEDocFile(`${environment.HOST.FILE_SYSTEM}/api/${this.VERSION}/en-US/EDoc/UploadEdoc`, body, files).pipe(
            map((data: any) => data)
        );
    }

    getEDocByJob(jobId: string, transitionType: string) {
        return this._api.get(`${environment.HOST.FILE_SYSTEM}/api/${this.VERSION}/en-US/EDoc/GetEDocByJob?jobId=${jobId}&transactionType=${transitionType}`).pipe(
            map((data: any) => data)
        );
    }

    deleteEdoc(edocId: string) {
        return this._api.delete(`${environment.HOST.FILE_SYSTEM}/api/${this.VERSION}/en-US/EDoc/DeleteEDoc/${edocId}`).pipe(
            map((data: any) => data)
        );
    }
}