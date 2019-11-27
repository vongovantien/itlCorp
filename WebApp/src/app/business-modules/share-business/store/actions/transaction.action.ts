import { Action } from '@ngrx/store';


export enum TransactionActionTypes {
    GET_DETAIL = '[Transaction] Get Detail',
    GET_DETAIL_SUCCESS = '[Transaction] Get Detail Success',
    GET_DETAIL_FAIL = '[Transaction] Get Detail Fail',

    UPDATE = '[Transaction] Update',
    UPDATE_SUCCESS = '[Transaction] Update Success',
    UPDATE_FAIL = '[Transaction] Update Fail',

    GET_PROFIT = '[Transaction] Get Profit',
    GET_PROFIT_SUCCESS = '[Transaction] Get Profit Success',
    GET_PROFIT_FAIL = '[Transaction] Get Profit Fail',
}

export class TransactionGetDetailAction implements Action {
    readonly type = TransactionActionTypes.GET_DETAIL;

    constructor(public payload: string) { }
}

export class TransactionGetDetailSuccessAction implements Action {
    readonly type = TransactionActionTypes.GET_DETAIL_SUCCESS;

    constructor(public payload: any) { }
}

export class TransactionGetDetailFailAction implements Action {
    readonly type = TransactionActionTypes.GET_DETAIL_FAIL;

    constructor(public payload: any) { }
}

export class TransactionUpdateAction implements Action {
    readonly type = TransactionActionTypes.UPDATE;

    constructor(public payload: any) { }
}

export class TransactionUpdateSuccessAction implements Action {
    readonly type = TransactionActionTypes.UPDATE_SUCCESS;

    constructor(public payload: any) { }
}

export class TransactionUpdateFailAction implements Action {
    readonly type = TransactionActionTypes.UPDATE_FAIL;

    constructor(public payload: any) { }
}

export class TransactionGetProfitAction implements Action {
    readonly type = TransactionActionTypes.GET_PROFIT;

    constructor(public payload: any) { }
}
export class TransactionGetProfitSuccessAction implements Action {
    readonly type = TransactionActionTypes.GET_PROFIT_SUCCESS;

    constructor(public payload: any) { }
}
export class TransactionGetProfitFailFailAction implements Action {
    readonly type = TransactionActionTypes.GET_PROFIT_FAIL;

    constructor(public payload: any) { }
}

export type TransactionActions =
    TransactionGetProfitAction
    | TransactionGetProfitSuccessAction
    | TransactionGetProfitFailFailAction
    | TransactionGetDetailAction
    | TransactionGetDetailSuccessAction
    | TransactionGetDetailFailAction
    | TransactionUpdateAction
    | TransactionUpdateSuccessAction
    | TransactionUpdateFailAction;

