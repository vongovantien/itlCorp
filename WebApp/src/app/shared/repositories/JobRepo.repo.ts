import { Injectable } from '@angular/core';
import { ApiService } from '../services/api.service';
import { environment } from 'src/environments/environment';

@Injectable()
export class JobRepo {

    private MODULE: string = 'operation';
    private VERSION: string = 'v1';
    // private baseApi: string = environment.HOST.WEB_URL;
    constructor(protected _api: ApiService) {
        // this.baseApi = _api.getApiUrl(this.baseApi, 44365, this.MODULE);
        //console.log(this.baseApi);
    }

    getListStageOfJob(jobId: string) {
        return this._api.get(`${environment.HOST.WEB_URL}44365/api/${this.VERSION}/vi/OpsStageAssigned/GetBy`, { jobId: jobId });
        // return this._api.get(`${environment.HOST.WEB_URL}/${this.MODULE}/api/${this.VERSION}/vi/OpsStageAssigned/GetBy`, { jobId: jobId });
    }

    getDetailStageOfJob(stageId: string) {
        return this._api.get(`${environment.HOST.WEB_URL}44365/api/${this.VERSION}/vi/OpsStageAssigned/${stageId}`);
        // return this._api.get(`${environment.HOST.WEB_URL}/${this.MODULE}/api/${this.VERSION}/vi/OpsStageAssigned/${stageId}`);
    }

    getListStageNotAssigned(jobId: string) {
        return this._api.get(`${environment.HOST.WEB_URL}44365/api/${this.VERSION}/vi/OpsStageAssigned/GetNotAssigned`, { jobId: jobId });
        // return this._api.get(`${environment.HOST.WEB_URL}/${this.MODULE}/api/${this.VERSION}/vi/OpsStageAssigned/GetNotAssigned`, { jobId: jobId });
    }

    addStageToJob(jobId: string, body: any = {}) {
        return this._api.put(`${environment.HOST.WEB_URL}44365/api/${this.VERSION}/vi/OpsStageAssigned/AddMultipleStage`, body, { jobId: jobId });
        // return this._api.put(`${environment.HOST.WEB_URL}/${this.MODULE}/api/${this.VERSION}/vi/OpsStageAssigned/AddMultipleStage`, body, { jobId: jobId });
    }

    updateStageToJob(body: any = {}) {
        return this._api.put(`${environment.HOST.WEB_URL}44365/api/${this.VERSION}/vi/OpsStageAssigned/Update`, body);
        // return this._api.put(`${environment.HOST.WEB_URL}/${this.MODULE}/api/${this.VERSION}/vi/OpsStageAssigned/Update`, body);

    }

    //#region ops job billing 
    addOPSJob(body: any = {}) {
        return this._api.post(`${environment.HOST.WEB_URL}44366/api/${this.VERSION}/vi/OpsTransaction/Add`, body);
        // return this._api.post(`${environment.HOST.WEB_URL}/Documentation/api/${this.VERSION}/vi/OpsTransaction/Add`, body);
    }
    //#endregion
}