import { Action, createReducer, on } from '@ngrx/store';
import * as Types from '../actions/';


export interface ISettlementPaymentSearchParamsState {
    searchParams: any;

}
export const initialState: ISettlementPaymentSearchParamsState = {
    searchParams: {}
};

const settlementPaymentReducer = createReducer(
    initialState,
    on(Types.SearchList, (state: ISettlementPaymentSearchParamsState, data: any) => ({
        ...state, searchParams: { ...data.payload }
    })),
);

export function reducer(state: any | undefined, action: Action) {
    return settlementPaymentReducer(state, action);
};