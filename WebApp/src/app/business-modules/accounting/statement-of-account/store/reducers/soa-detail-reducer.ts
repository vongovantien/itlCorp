import { SOA } from '@models';
import { Action, createReducer, on } from '@ngrx/store';
import * as Types from '../actions';

export interface SOADetailState {
    data: SOA;
    isLoading: boolean;
    isLoaded: boolean;
}

export const initialState: SOADetailState = {
    data: new SOA(),
    isLoading: true,
    isLoaded: true
};


const SOADetailReducer = createReducer(
    initialState,
    on(
        Types.LoadSOADetailSuccess, (state: SOADetailState, payload: { detail: SOA }) => {
            console.log(payload);
            return { ...state, data: payload.detail, isLoading: false, isLoaded: true };
        }
    )
);

export function soaDetailReducer(state: any | undefined, action: Action) {
    return SOADetailReducer(state, action);
};
