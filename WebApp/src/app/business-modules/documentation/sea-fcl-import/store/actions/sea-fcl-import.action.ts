import { Action } from '@ngrx/store';

/**
 * For each action type in an action group, make a simple
 * enum object for all of this group's action types.
 */

export enum SeaFCLImportActionTypes {
    GET_DETAIL = '[FCL Import] Get Detail',
    GET_DETAIL_SUCCESS = '[FCL Import] Get Detail Success',
    GET_DETAIL_FAIL = '[FCL Import] Get Detail Fail',

    UPDATE = '[FCL Import] Update',
    UPDATE_SUCCESS = '[FCL Import] Update Success',
    UPDATE_FAIL = '[FCL Import] Update Fail',

}

/**
 * Every action is comprised of at least a type and an optional
 * payload. Expressing actions as classes enables powerful 
 * type checking in reducer functions.
 */
export class SeaFCLImportGetDetailAction implements Action {
    readonly type = SeaFCLImportActionTypes.GET_DETAIL;

    constructor(public payload: any) { }
}

export class SeaFCLImportGetDetailSuccessAction implements Action {
    readonly type = SeaFCLImportActionTypes.GET_DETAIL_SUCCESS;

    constructor(public payload: any) { }
}
export class SeaFCLImportGetDetailFailAction implements Action {
    readonly type = SeaFCLImportActionTypes.GET_DETAIL_FAIL;

    constructor(public payload: any) { }
}
export class SeaFCLImportUpdateAction implements Action {
    readonly type = SeaFCLImportActionTypes.UPDATE;

    constructor(public payload: any) { }
}
export class SeaFCLImportUpdateSuccessAction implements Action {
    readonly type = SeaFCLImportActionTypes.UPDATE_SUCCESS;

    constructor(public payload: any) { }
}
export class SeaFCLImportUpdateFailAction implements Action {
    readonly type = SeaFCLImportActionTypes.UPDATE_FAIL;

    constructor(public payload: any) { }
}


/**
 * Export a type alias of all actions in this action group
 * so that reducers can easily compose action types
 */
export type SeaFCLImportActions =
    | SeaFCLImportUpdateAction
    | SeaFCLImportUpdateSuccessAction
    | SeaFCLImportUpdateFailAction
    | SeaFCLImportGetDetailAction
    | SeaFCLImportGetDetailSuccessAction
    | SeaFCLImportGetDetailFailAction;
