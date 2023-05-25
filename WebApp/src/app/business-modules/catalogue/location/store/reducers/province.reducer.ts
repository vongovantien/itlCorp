import { Action, createReducer, on } from '@ngrx/store';
import * as Types from '../actions/province.action';


export interface ICityListState {
    cities: any;
    isLoading: boolean;
    isLoaded: boolean;
    dataSearch: any;
    pagingData: any;
    
}

export const initialState: ICityListState = {
    cities: { data: [], totalItems: 0, },
    isLoading: false,
    isLoaded: false,
    pagingData: { page: 1, pageSize: 15 },
    dataSearch: {}
};

const CityReducer = createReducer(
    initialState,
    on(Types.SearchListCity, (state: ICityListState, data: any) => ({
        ...state, dataSearch: { ...data.payload }
    })),
    on(
        Types.LoadListCityLocation, (state: ICityListState, payload: CommonInterface.IParamPaging) => {
            return { ...state, isLoading: true, pagingData: { page: payload.page, pageSize: payload.size } };
        }
    ),
    on(
        Types.LoadListCitySuccess, (state: ICityListState, payload: CommonInterface.IResponsePaging) => {
            return { ...state, cities: payload, isLoading: false, isLoaded: true };
        }
    )
);

export function ciReducer(state: any | undefined, action: Action) {
    return CityReducer(state, action);
};