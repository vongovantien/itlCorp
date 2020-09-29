import { Action, createReducer, on } from '@ngrx/store';
import * as Types from '../actions/';


export interface IAdvancePaymentSearchParamsState {
    searchParams: any;

}
export const initialState: IAdvancePaymentSearchParamsState = {
    searchParams: {}
};

const advancePaymentReducer = createReducer(
    initialState,
    on(Types.SearchList, (state: IAdvancePaymentSearchParamsState, data: any) => ({
        ...state, searchParams: { ...data.payload }
    })),
);

export function reducer(state: any | undefined, action: Action) {
    return advancePaymentReducer(state, action);
};