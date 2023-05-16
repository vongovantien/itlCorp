import { createAction, props } from '@ngrx/store';

export enum CountryActionTypes {
    SEARCH_LIST = '[CountryAction] Search List',
    LOAD_LIST_COUNTRY = '[CountryAction] Load List',
    LOAD_LIST_COUNTRY_SUCCESS = '[CountryAction] Load List Success]'
};

export const SearchListCountry = createAction(CountryActionTypes.SEARCH_LIST, props<{ payload: any }>());
export const LoadListCountryLocation = createAction(CountryActionTypes.LOAD_LIST_COUNTRY, props<CommonInterface.IParamPaging>());
export const LoadListCountrySuccess = createAction(CountryActionTypes.LOAD_LIST_COUNTRY_SUCCESS, props<CommonInterface.IResponsePaging>());