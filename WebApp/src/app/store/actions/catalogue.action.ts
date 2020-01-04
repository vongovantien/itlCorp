import { Action } from '@ngrx/store';


export enum CatalogueActionTypes {
    GET_PORT = '[Catalogue] Get Port',
    GET_PORT_SUCCESS = '[Catalogue] Get Port Success',
    GET_PORT_FAIL = '[Catalogue] Get Port Fail',
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


export type CatalogueActions = GetCataloguePortAction
    | GetCataloguePortSuccessAction
    | GetCataloguePortFailAction
    ;
