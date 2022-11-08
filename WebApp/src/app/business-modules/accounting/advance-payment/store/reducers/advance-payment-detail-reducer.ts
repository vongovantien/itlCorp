import { AdvancePayment } from '@models';
import { Action, createReducer, on } from '@ngrx/store';
import * as Types from '../actions/';

export interface AdvancePaymentDetailState {
    data: AdvancePayment;
    isLoading: boolean;
    isLoaded: boolean;
}

export const initialState: AdvancePaymentDetailState = {
    data: new AdvancePayment(),
    isLoading: true,
    isLoaded: true
};


const AdvancePaymentDetailReducer = createReducer(
    initialState,
    on(
        Types.LoadAdvanceDetailSuccess, (state: AdvancePaymentDetailState, payload: AdvancePayment) => {
            return { ...state, data: payload, isLoading: false, isLoaded: true };
        }
    )
);

export function advancePaymentDetailReducer(state: any | undefined, action: Action) {
    return AdvancePaymentDetailReducer(state, action);
};