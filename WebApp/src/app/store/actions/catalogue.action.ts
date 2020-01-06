import { Action } from '@ngrx/store';


export enum CatalogueActionTypes {

    GET_PARTNER = '[Catalogue] Get Partner',

    GET_PORT = '[Catalogue] Get Port',
    GET_PORT_SUCCESS = '[Catalogue] Get Port Success',
    GET_PORT_FAIL = '[Catalogue] Get Port Fail',

    GET_CARRIER = '[Catalogue] Get Carrier',
    GET_CARRIER_SUCCESS = '[Catalogue] Get Carrier Success',
    GET_CARRIER_FAIL = '[Catalogue] Get Carrier Fail',

    GET_AGENT = '[Catalogue] Get Agent',
    GET_AGENT_SUCCESS = '[Catalogue] Get Agent Success',
    GET_AGENT_FAIL = '[Catalogue] Get Agent Fail',
}

export class GetCataloguePartnerAction implements Action {
    readonly type = CatalogueActionTypes.GET_PARTNER;
    constructor(public payload: number) {
    }
}
export class GetCataloguePortAction implements Action {
    readonly type = CatalogueActionTypes.GET_PORT;

    constructor(public payload: any) { }
}

export class GetCataloguePortSuccessAction implements Action {
    readonly type = CatalogueActionTypes.GET_PORT_SUCCESS;

    constructor(public payload: any) { }
}

export class GetCataloguePortFailAction implements Action {
    readonly type = CatalogueActionTypes.GET_PORT_FAIL;

    constructor(public payload: any) { }
}

export class GetCatalogueCarrierAction implements Action {
    readonly type = CatalogueActionTypes.GET_CARRIER;

    constructor(public payload: any) { }
}

export class GetCatalogueCarrierSuccessAction implements Action {
    readonly type = CatalogueActionTypes.GET_CARRIER_SUCCESS;

    constructor(public payload: any) { }
}

export class GetCatalogueCarrierFailAction implements Action {
    readonly type = CatalogueActionTypes.GET_CARRIER_FAIL;

    constructor(public payload: any) { }
}

export class GetCatalogueAgentAction implements Action {
    readonly type = CatalogueActionTypes.GET_AGENT;

    constructor(public payload: any) { }
}

export class GetCatalogueAgentSuccessAction implements Action {
    readonly type = CatalogueActionTypes.GET_AGENT_SUCCESS;

    constructor(public payload: any) { }
}

export class GetCatalogueAgentFailAction implements Action {
    readonly type = CatalogueActionTypes.GET_AGENT_FAIL;

    constructor(public payload: any) { }
}


export type CatalogueActions = GetCataloguePartnerAction
    | GetCataloguePortAction
    | GetCataloguePortSuccessAction
    | GetCataloguePortFailAction
    | GetCatalogueCarrierAction
    | GetCatalogueCarrierSuccessAction
    | GetCatalogueCarrierFailAction
    | GetCatalogueAgentAction
    | GetCatalogueAgentSuccessAction
    | GetCatalogueAgentFailAction
    ;
