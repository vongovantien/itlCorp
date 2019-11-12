import { Action } from '@ngrx/store';

export enum HouseBillActionTypes {
    GET_DETAIL = '[HBL] Get Detail',
    GET_DETAIL_SUCCESS = '[HBL] Get Detail Success',
    GET_DETAIL_FAIL = '[HBL] Get Detail Fail',

    GET_PROFIT = '[HBL] Get Profit',
    GET_PROFIT_SUCCESS = '[HBL] Get Profit Success',
    GET_PROFIT_FAIL = '[HBL] Get Profit Fail'
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

export class GetProfitHBLAction implements Action {
    readonly type = HouseBillActionTypes.GET_PROFIT;

    constructor(public payload: any) { }
}

export class GetProfitHBLSuccessAction implements Action {
    readonly type = HouseBillActionTypes.GET_PROFIT_SUCCESS;

    constructor(public payload: any) { }
}

export class GetProfitHBLFailAction implements Action {
    readonly type = HouseBillActionTypes.GET_PROFIT_FAIL;

    constructor(public payload: any) { }
}

export type HBlActions =
    GetDetailHBLAction
    | GetDetailHBLSuccessAction
    | GetDetailHBLFailAction
    | GetProfitHBLAction
    | GetProfitHBLSuccessAction
    | GetProfitHBLFailAction;
