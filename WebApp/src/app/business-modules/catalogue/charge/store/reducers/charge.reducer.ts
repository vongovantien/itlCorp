import { Action, createReducer, on } from '@ngrx/store';
import * as Types from '../actions/';


export interface IChargeSearchParamsState {
    searchParams: any;

}
export const initialState: IChargeSearchParamsState = {
    searchParams: {}
};

const chargeReducer = createReducer(
    initialState,
    on(Types.SearchList, (state: IChargeSearchParamsState, data: any) => ({
        ...state, searchParams: { ...data.payload }
    })),
);

export function reducer(state: any | undefined, action: Action) {
    return chargeReducer(state, action);
};