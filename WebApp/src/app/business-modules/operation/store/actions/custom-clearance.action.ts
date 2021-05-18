import { Action } from '@ngrx/store';


export enum ClearanceActionTypes {
    SEARCH_LIST = '[Custom Declaration] Search List',

    LOAD_LIST = '[Custom Declaration] Load List',
    LOAD_LIST_SUCCESS = 'Custom Declaration] Load Success',
    LOAD_LIST_FAIL = '[Custom Declaration] Load Fail',

    GET_DETAIL = '[Custom Declaration] Get Detail',
    GET_DETAIL_SUCCESS = '[Custom Declaration] Get Detail Success',
    GET_DETAIL_FAIL = '[Custom Declaration] Get Detail Fail',
};

export class CustomsDeclarationSearchListAction implements Action {
    readonly type = ClearanceActionTypes.SEARCH_LIST;
    constructor(public payload: any) { }
}
export class CustomsDeclarationLoadListAction implements Action {
    readonly type = ClearanceActionTypes.LOAD_LIST;
    constructor(public payload: CommonInterface.IParamPaging) { }
}
export class CustomsDeclarationLoadListSuccessAction implements Action {
    readonly type = ClearanceActionTypes.LOAD_LIST_SUCCESS;
    constructor(public payload: any) { }
}


/**
 * Export a type alias of all actions in this action group
 * so that reducers can easily compose action types
 */
export type ClearanceActions
    = CustomsDeclarationSearchListAction
    | CustomsDeclarationLoadListAction
    | CustomsDeclarationLoadListSuccessAction;
