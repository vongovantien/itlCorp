import { Action } from '@ngrx/store';

export enum HouseBillActionTypes {
    GET_DETAIL = '[HBL] Get Detail',
    GET_DETAIL_SUCCESS = '[HBL] Get Detail Success',
    GET_DETAIL_FAIL = '[HBL] Get Detail Fail'
}


export class GetDetailHBLAction implements Action {
    readonly type = HouseBillActionTypes.GET_DETAIL;

    constructor(public payload: any) { }
}

export class GetDetailHBLSuccessAction implements Action {
    readonly type = HouseBillActionTypes.GET_DETAIL_SUCCESS;

    constructor(public payload: any) { }
}

export class GetDetailHBLFailAction implements Action {
    readonly type = HouseBillActionTypes.GET_DETAIL_FAIL;

    constructor(public payload: any) { }
}

export type HBlAction = GetDetailHBLAction | GetDetailHBLSuccessAction | GetDetailHBLFailAction;