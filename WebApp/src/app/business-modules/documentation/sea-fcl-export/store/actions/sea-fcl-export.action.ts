import { Action } from '@ngrx/store';

export enum SeaFCLExportTypes {
    LOAD_LIST = '[FCL Export] Load List',
    LOAD_LIST_SUCCESS = '[FCL Export] Load Success',
    LOAD_LIST_FAIL = '[FCL Export] Load Fail',
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



export type SeaFCLExportActions =
    SeaFCLExportLoadAction
    | SeaFCLExportLoadSuccessAction
    | SeaFCLExportLoadFailAction;
