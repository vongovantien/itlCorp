import { createAction, props } from '@ngrx/store';

export enum CityActionTypes {
    SEARCH_LIST = '[CityAction] Search List',
    LOAD_LIST_CITY = '[CityAction] Load List',
    LOAD_LIST_CITY_SUCCESS = '[CityAction] Load List Success]'
};

export const SearchListCity = createAction(CityActionTypes.SEARCH_LIST, props<{ payload: any }>());
export const LoadListCityLocation = createAction(CityActionTypes.LOAD_LIST_CITY, props<CommonInterface.IParamPaging>());
export const LoadListCitySuccess = createAction(CityActionTypes.LOAD_LIST_CITY_SUCCESS, props<CommonInterface.IResponsePaging>());