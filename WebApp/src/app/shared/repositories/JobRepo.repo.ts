import { Injectable } from '@angular/core';
import { ApiService } from '../services/api.service';
import { environment } from 'src/environments/environment';
import { map } from 'rxjs/operators';

@Injectable()
export class JobRepo {

    private MODULE: string = 'operation';
    private VERSION: string = 'v1';
    constructor(protected _api: ApiService) {
    }

    getListStageOfJob(jobId: string) {
        // return this._api.get(`${environment.HOST.WEB_URL}44365/api/${this.VERSION}/vi/OpsStageAssigned/GetBy`, { jobId: jobId });
        return this._api.get(`${environment.HOST.WEB_URL}/${this.MODULE}/api/${this.VERSION}/vi/OpsStageAssigned/GetBy`, { jobId: jobId });
    }

    getDetailStageOfJob(stageId: string) {
        // return this._api.get(`${environment.HOST.WEB_URL}44365/api/${this.VERSION}/vi/OpsStageAssigned/${stageId}`);
        return this._api.get(`${environment.HOST.WEB_URL}/${this.MODULE}/api/${this.VERSION}/vi/OpsStageAssigned/${stageId}`);
    }

    getListStageNotAssigned(jobId: string) {
        // return this._api.get(`${environment.HOST.WEB_URL}44365/api/${this.VERSION}/vi/OpsStageAssigned/GetNotAssigned`, { jobId: jobId });
        return this._api.get(`${environment.HOST.WEB_URL}/${this.MODULE}/api/${this.VERSION}/vi/OpsStageAssigned/GetNotAssigned`, { jobId: jobId });
    }

    addStageToJob(jobId: string, body: any = {}) {
        // return this._api.put(`${environment.HOST.WEB_URL}44365/api/${this.VERSION}/vi/OpsStageAssigned/AddMultipleStage`, body, { jobId: jobId });
        return this._api.put(`${environment.HOST.WEB_URL}/${this.MODULE}/api/${this.VERSION}/vi/OpsStageAssigned/AddMultipleStage`, body, { jobId: jobId });
    }

    updateStageToJob(body: any = {}) {
        // return this._api.put(`${environment.HOST.WEB_URL}44365/api/${this.VERSION}/vi/OpsStageAssigned/Update`, body);
        return this._api.put(`${environment.HOST.WEB_URL}/${this.MODULE}/api/${this.VERSION}/vi/OpsStageAssigned/Update`, body);

    }

    //#region ops job billing 
    addOPSJob(body: any = {}) {
        // return this._api.post(`${environment.HOST.WEB_URL}44366/api/${this.VERSION}/vi/OpsTransaction/Add`, body);
        return this._api.post(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/vi/OpsTransaction/Add`, body);
    }
    //#endregion

    previewPL(jobId, currency) {
        return this._api.post(`localhost:44366/api/${this.VERSION}/en-US/OpsTransaction/PreviewFormPLsheet`, null, { jobId: jobId, currency: currency }, {
            Authorization: 'Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IjhFMTI2MzEwN0VDMUE2RkUxQkIxMjZEREM5QzM5MDVGNkQ4MkIyNjQiLCJ0eXAiOiJKV1QiLCJ4NXQiOiJqaEpqRUg3QnB2NGJzU2JkeWNPUVgyMkNzbVEifQ.eyJuYmYiOjE1NjkyMzIwODMsImV4cCI6MTU2OTI0NjQ4MywiaXNzIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6NDQzNjkiLCJhdWQiOlsiaHR0cHM6Ly9sb2NhbGhvc3Q6NDQzNjkvcmVzb3VyY2VzIiwiZG50X2FwaSJdLCJjbGllbnRfaWQiOiJlRk1TIiwic3ViIjoiYWRtaW4iLCJhdXRoX3RpbWUiOjE1NjkyMzIwODMsImlkcCI6ImxvY2FsIiwiaWQiOiJhZG1pbiIsImVtYWlsIjoiYW5keS5ob2FAaXRsdm4uY29tIiwicHJlZmVycmVkX3VzZXJuYW1lIjoiYWRtaW4iLCJwaG9uZV9udW1iZXIiOiIrODQxNjY3MjYzNTM2Iiwid29ya3BsYWNlSWQiOiIiLCJ1c2VyTmFtZSI6ImFkbWluIiwiZW1wbG95ZWVJZCI6IkQxRkMzM0E5LUY5MzctNEVDMC04MzQzLTAwMDgzQTQyNEU5OCIsInNjb3BlIjpbImVmbXNfc2NvcGUiLCJvcGVuaWQiLCJwcm9maWxlIiwiZG50X2FwaSIsIm9mZmxpbmVfYWNjZXNzIl0sImFtciI6WyJjdXN0b20iXX0.Q-2Gs6n8K_HzoXKqjPEF-tNp6gyVmox2Ma5Q_ObVjFr_IQjupC3VZTmoO2bycCtigJ3XEasoj0nXtpe1NHRIysN7A2mjE_pxsM2Gp9_3CEKTv5SVxVHesew4oxo2FkwVVMYpBJDPCxcGQr-KZA9MiE10X4Dd9eK_N7BpELGmOH_wlSbU1SGjQO1AvpI27KpXVGGUJFn_0H2BribIZuOBA3XaqGh2I4cSSjOHVRvP4coprY44VZiw_uiaJMk8s-r_dhNfwmhOlUOdH_3Mi7RS9NKJYQEOQVtf6WuPBiGZf96zhdsQ6p3EjeTgOhpMfGRqWe-a23erm6qutIboR3_iIQ'
        }).pipe(
            map((data: any) => data)
        );
        // return this._api.get(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/en-US/OpsTransaction/PreviewFormPLsheet`, { jobId: jobId, currency: currency }).pipe(
        //     map((data: any) => data)
        // );
    }
}
