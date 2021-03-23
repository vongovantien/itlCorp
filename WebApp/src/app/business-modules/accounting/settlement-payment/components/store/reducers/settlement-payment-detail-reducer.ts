import { createReducer, on, Action } from '@ngrx/store';
import * as Actions from './../actions';
import { ISettlementPaymentData } from '../../../detail/detail-settlement-payment.component';




export interface SettlePaymentDetailState {
    settlement: ISettlementPaymentData;
    isLoading: boolean;
    isLoaded: boolean;
}

export const initialState: SettlePaymentDetailState = {
    settlement: null,
    isLoading: false,
    isLoaded: false
};

const reducer = createReducer(
    initialState,
    on(Actions.LoadDetailSettlePaymentSuccess, (state: SettlePaymentDetailState, payload: ISettlementPaymentData) => ({
        ...state, settlement: payload
    }))
);

export function settlePaymentDetailreducer(state: any | undefined, action: Action) {
    return reducer(state, action);
}