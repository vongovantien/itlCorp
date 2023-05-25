import { Action, createReducer, on } from '@ngrx/store';
import * as Types from '../actions/district.action';


export interface IDistrictListState {
    districts: any;
    isLoading: boolean;
    isLoaded: boolean;
    dataSearch: any;
    pagingData: any;
    
}

export const initialState: IDistrictListState = {
    districts: { data: [], totalItems: 0, },
    isLoading: false,
    isLoaded: false,
    pagingData: { page: 1, pageSize: 15 },
    dataSearch: {}
};

const districtReducer = createReducer(
    initialState,
    on(Types.SearchListDistrict, (state: IDistrictListState, data: any) => ({
        ...state, dataSearch: { ...data.payload }
    })),
    on(
        Types.LoadListDistrictLocation, (state: IDistrictListState, payload: CommonInterface.IParamPaging) => {
            return { ...state, isLoading: true, pagingData: { page: payload.page, pageSize: payload.size } };
        }
    ),
    on(
        Types.LoadListDistrictSuccess, (state: IDistrictListState, payload: CommonInterface.IResponsePaging) => {
            return { ...state, districts: payload, isLoading: false, isLoaded: true };
        }
    )
);

export function disReducer(state: any | undefined, action: Action) {
    return districtReducer(state, action);
};