import { Action } from '@ngrx/store';


export enum TransactionActionTypes {
    GET_PROFIT = '[Transaction] Get Profit',
    GET_PROFIT_SUCCESS = '[Transaction] Get Profit Success',
    GET_PROFIT_FAIL = '[Transaction] Get Profit Fail',
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
    | TransactionGetProfitFailFailAction;

