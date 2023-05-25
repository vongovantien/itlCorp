import { Action, createReducer, on } from '@ngrx/store';
import * as Types from '../actions/country.action';


export interface ICountryListState {
    countries: any;
    isLoading: boolean;
    isLoaded: boolean;
    dataSearch: any;
    pagingData: any;
    
}

export const initialState: ICountryListState = {
    countries: { data: [], totalItems: 0, },
    isLoading: false,
    isLoaded: false,
    pagingData: { page: 1, pageSize: 15 },
    dataSearch: {}
};

const CountryReducer = createReducer(
    initialState,
    on(Types.SearchListCountry, (state: ICountryListState, data: any) => ({
        ...state, dataSearch: { ...data.payload }
    })),
    on(
        Types.LoadListCountryLocation, (state: ICountryListState, payload: CommonInterface.IParamPaging) => {
            return { ...state, isLoading: true, pagingData: { page: payload.page, pageSize: payload.size } };
        }
    ),
    on(
        Types.LoadListCountrySuccess, (state: ICountryListState, payload: CommonInterface.IResponsePaging) => {
            return { ...state, countries: payload, isLoading: false, isLoaded: true };
        }
    )
);

export function couReducer(state: any | undefined, action: Action) {
    return CountryReducer(state, action);
};