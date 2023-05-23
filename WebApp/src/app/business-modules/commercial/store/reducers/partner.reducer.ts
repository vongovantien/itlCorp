import { Action, createReducer, on } from '@ngrx/store';
import * as Types from '../actions';


export interface IPartnerState {
    customer: any;
    isLoading: boolean;
    isLoaded: boolean;
}

export const initialState: IPartnerState = {
    customer: {},
    isLoading: false,
    isLoaded: false,
};

const reducer = createReducer(
    initialState,
    on(
        Types.getDetailPartner, (state: IPartnerState, payload: any) => {
            return { ...state, payload, isLoading: true };
        }
    ),
    on(
        Types.getDetailPartnerSuccess, (state: IPartnerState, payload: any) => {
            console.log(payload)
            return { ...state, customer: payload.payload, isLoading: false, isLoaded: true };
        }
    ),
    on(
        Types.getDetailPartnerFail, (state: IPartnerState) => {
            return { ...state, isLoading: false, isLoaded: true };
        }
    ),
);

export function partnerReducer(state: any | undefined, action: Action) {
    return reducer(state, action);
};
