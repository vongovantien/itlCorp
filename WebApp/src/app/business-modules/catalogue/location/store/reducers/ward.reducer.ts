import { Action, createReducer, on } from '@ngrx/store';
import * as Types from '../actions/ward.action';


export interface IWardListState {
    wards: any;
    isLoading: boolean;
    isLoaded: boolean;
    dataSearch: any;
    pagingData: any;
    
}

export const initialState: IWardListState = {
    wards: { data: [], totalItems: 0, },
    isLoading: false,
    isLoaded: false,
    pagingData: { page: 1, pageSize: 15 },
    dataSearch: {}
};

const WardReducer = createReducer(
    initialState,
    on(Types.SearchListWard, (state: IWardListState, data: any) => ({
        ...state, dataSearch: { ...data.payload }
    })),
    on(
        Types.LoadListWardLocation, (state: IWardListState, payload: CommonInterface.IParamPaging) => {
            return { ...state, isLoading: true, pagingData: { page: payload.page, pageSize: payload.size } };
        }
    ),
    on(
        Types.LoadListWardSuccess, (state: IWardListState, payload: CommonInterface.IResponsePaging) => {
            return { ...state, wards: payload, isLoading: false, isLoaded: true };
        }
    )
);

export function waReducer(state: any | undefined, action: Action) {
    return WardReducer(state, action);
};