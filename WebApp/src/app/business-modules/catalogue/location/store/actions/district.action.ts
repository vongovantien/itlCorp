import { createAction, props } from '@ngrx/store';

export enum DistrictActionTypes {
    SEARCH_LIST_DISTRICT = '[DistrictAction] Search List',
    LOAD_LIST_DISTRICT = '[DistrictAction] Load List',
    LOAD_LIST_DISTRICT_SUCCESS = '[DistrictAction] Load List Success]'
};

export const SearchListDistrict = createAction(DistrictActionTypes.SEARCH_LIST_DISTRICT, props<{ payload: any }>());
export const LoadListDistrictLocation = createAction(DistrictActionTypes.LOAD_LIST_DISTRICT, props<CommonInterface.IParamPaging>());
export const LoadListDistrictSuccess = createAction(DistrictActionTypes.LOAD_LIST_DISTRICT_SUCCESS, props<CommonInterface.IResponsePaging>());