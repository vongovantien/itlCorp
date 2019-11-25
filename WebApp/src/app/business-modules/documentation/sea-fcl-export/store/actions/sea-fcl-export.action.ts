import { Action } from '@ngrx/store';

export enum SeaFCLExportTypes {

    LOAD_LIST = '[FCL Export] Load List',
    LOAD_LIST_SUCCESS = '[FCL Export] Load Success',
    LOAD_LIST_FAIL = '[FCL Export] Load Fail',

    GET_DETAIL = '[FCL Export] Get Detail',
    GET_DETAIL_SUCCESS = '[FCL Export] Get Detail Success',
    GET_DETAIL_FAIL = '[FCL Export] Get Detail Fail',

    UPDATE = '[FCL Export] Update',
    UPDATE_SUCCESS = '[FCL Export] Update Success',
    UPDATE_FAIL = '[FCL Export] Update Fail',
}

export class SeaFCLExportLoadAction implements Action {
    readonly type = SeaFCLExportTypes.LOAD_LIST;
    constructor(public payload: any) { }
}

export class SeaFCLExportLoadSuccessAction implements Action {
    readonly type = SeaFCLExportTypes.LOAD_LIST_SUCCESS;
    constructor(public payload: any) { }
}


export class SeaFCLExportLoadFailAction implements Action {
    readonly type = SeaFCLExportTypes.LOAD_LIST_FAIL;

    constructor(public payload: any) { }
}

export class SeaFCLExportGetDetailAction implements Action {
    readonly type = SeaFCLExportTypes.GET_DETAIL;

    constructor(public payload: any) { }
}

export class SeaFCLExportGetDetailSuccessAction implements Action {
    readonly type = SeaFCLExportTypes.GET_DETAIL_SUCCESS;

    constructor(public payload: any) { }
}
export class SeaFCLExportGetDetailFailAction implements Action {
    readonly type = SeaFCLExportTypes.GET_DETAIL_FAIL;

    constructor(public payload: any) { }
}
export class SeaFCLExportUpdateAction implements Action {
    readonly type = SeaFCLExportTypes.UPDATE;

    constructor(public payload: any) { }
}
export class SeaFCLExportUpdateSuccessAction implements Action {
    readonly type = SeaFCLExportTypes.UPDATE_SUCCESS;

    constructor(public payload: any) { }
}
export class SeaFCLExportUpdateFailAction implements Action {
    readonly type = SeaFCLExportTypes.UPDATE_FAIL;

    constructor(public payload: any) { }
}



export type SeaFCLExportActions =
    SeaFCLExportLoadAction
    | SeaFCLExportLoadSuccessAction
    | SeaFCLExportLoadFailAction
    | SeaFCLExportGetDetailAction
    | SeaFCLExportGetDetailSuccessAction
    | SeaFCLExportGetDetailFailAction
    | SeaFCLExportUpdateAction
    | SeaFCLExportUpdateSuccessAction
    | SeaFCLExportUpdateFailAction;
