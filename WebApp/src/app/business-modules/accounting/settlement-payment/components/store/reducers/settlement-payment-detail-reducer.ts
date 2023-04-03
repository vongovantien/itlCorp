import { Action, createReducer, on } from '@ngrx/store';
import { ISettlementPaymentData } from '../../../detail/detail-settlement-payment.component';
import * as Actions from '../actions';
export interface SettlePaymentDetailState {
    settlement: ISettlementPaymentData;
    isLoading: boolean;
    isLoaded: boolean;
    eDocs: any[];
}

export const initialState: SettlePaymentDetailState = {
    settlement: null,
    isLoading: false,
    isLoaded: false,
    eDocs: []
};

const reducer = createReducer(
    initialState,
    on(Actions.LoadDetailSettlePayment, (state: SettlePaymentDetailState) => ({
        ...state, isLoading: true, isLoaded: false
    })),
    on(Actions.LoadDetailSettlePaymentSuccess, (state: SettlePaymentDetailState, payload: ISettlementPaymentData) => ({
        ...state, settlement: payload, isLoading: false, isLoaded: true
    })),
    on(Actions.LoadDetailSettlePaymentFail, (state: SettlePaymentDetailState) => ({
        ...state, isLoading: false, isLoaded: true
    })),
    on(Actions.UpdateListNoGroupSurcharge, (state: SettlePaymentDetailState, payload: any) => ({
        ...state, settlement: { ...state.settlement, chargeNoGrpSettlement: payload.data }
    })),
    on(Actions.LoadListNoGroupSurcharge, (state: SettlePaymentDetailState) => ({
        ...state, isLoading: true
    })),
    on(Actions.LoadListEDocSettleSuccess, (state: SettlePaymentDetailState, payload: any) => ({
        ...state, eDocs: payload.eDocs
    })),
);

export function settlePaymentDetailreducer(state: any | undefined, action: Action) {
    return reducer(state, action);
}
