import { Action, createReducer, on } from '@ngrx/store';
import * as Types from '../actions';

export interface ICustomerState {
    customers: any;
    isLoading: boolean;
    isLoaded: boolean;
    dataSearch: any;
    pagingData: any;
}

export const initialState: ICustomerState = {
    customers: { data: [], totalItems: 0, },
    isLoading: false,
    isLoaded: false,
    pagingData: { page: 1, pageSize: 15 },
    dataSearch: {}
};

const reducer = createReducer(
    initialState,
    on(Types.SearchList, (state: ICustomerState, data: any) => ({
        ...state, dataSearch: { ...data.payload }
    })),
    on(
        Types.LoadListCustomer, (state: ICustomerState, payload: CommonInterface.IParamPaging) => {

            return { ...state, isLoading: true, pagingData: { page: payload.page, pageSize: payload.size } };
        }
    ),
    on(
        Types.LoadListCustomerSuccess, (state: ICustomerState, payload: CommonInterface.IResponsePaging) => {
            return { ...state, customers: payload, isLoading: false, isLoaded: true };
        }
    ),
);

export function customerReducer(state: any | undefined, action: Action) {
    return reducer(state, action);
};
