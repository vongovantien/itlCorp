import { Action, createReducer, on } from '@ngrx/store';
import * as Types from '../actions';


export interface IPartnerDataState {
    partners: any;
    partner: any;
    isLoading: boolean;
    isLoaded: boolean;
    dataSearch: any;
    pagingData: any;
}

export const initialState: IPartnerDataState = {
    partners: { data: [], totalItems: 0, },
    partner: {},
    isLoading: false,
    isLoaded: false,
    pagingData: { page: 1, pageSize: 15 },
    dataSearch: {}
};

const reducer = createReducer(
    initialState,
    on(
        Types.getDetailPartner, (state: IPartnerDataState, payload: any) => {
            return { ...state, payload, isLoading: true };
        }
    ),
    on(
        Types.getDetailPartnerSuccess, (state: IPartnerDataState, data: any) => {
            return { ...state, partner: data.payload, isLoading: false, isLoaded: true };
        }
    ),
    on(
        Types.getDetailPartnerFail, (state: IPartnerDataState) => {
            return { ...state, isLoading: false, isLoaded: true };
        }
    ),
    on(Types.SearchListPartner, (state: IPartnerDataState, data: any) => ({
        ...state, dataSearch: { ...data.payload }
    })),
    on(
        Types.LoadListPartner, (state: IPartnerDataState, payload: CommonInterface.IParamPaging) => {
            return { ...state, isLoading: true, pagingData: { page: payload.page, pageSize: payload.size } };
        }
    ),
    on(
        Types.LoadListPartnerSuccess, (state: IPartnerDataState, payload: CommonInterface.IResponsePaging) => {
            return { ...state, partners: payload, isLoading: false, isLoaded: true };
        }
    )
);

export function partnerReducer(state: any | undefined, action: Action) {
    return reducer(state, action);
};
