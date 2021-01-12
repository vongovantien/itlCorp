import { Action, createReducer, on } from '@ngrx/store';
import * as Types from '../actions';


export interface ICustomerSearchParamsState {
    searchParams: any;

}
export const initialState: ICustomerSearchParamsState = {
    searchParams: {}
};

const customerReducer = createReducer(
    initialState,
    on(Types.SearchList, (state: ICustomerSearchParamsState, data: any) => ({
        ...state, searchParams: { ...data.payload }
    })),
);

export function reducer(state: any | undefined, action: Action) {
    return customerReducer(state, action);
};