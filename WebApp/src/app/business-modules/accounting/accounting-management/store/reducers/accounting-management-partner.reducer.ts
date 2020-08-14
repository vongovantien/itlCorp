import { createReducer, on, Action, State } from '@ngrx/store';
import * as accountingManagementActions from '../actions/accounting-management.action';
import { ChargeOfAccountingManagementModel, PartnerOfAcctManagementResult } from '@models';

export interface IAccountingManagementPartnerState {
    partnerId: string;
    partnerName: string;
    partnerAddress: string;
    settlementRequesterId: string;
    settlementRequester: string;
    inputRefNo: string;
    paymentTerm: number;
    charges: ChargeOfAccountingManagementModel[];
}

export const initialState: IAccountingManagementPartnerState = {
    partnerId: null,
    partnerName: null,
    partnerAddress: null,
    settlementRequesterId: null,
    settlementRequester: null,
    inputRefNo: null,
    charges: [],
    paymentTerm: null
};

const accountingManagementPartnerReducer = createReducer(
    initialState,
    on(accountingManagementActions.SelectPartner, (state: IAccountingManagementPartnerState, payload: PartnerOfAcctManagementResult) => ({
        ...state, charges: payload.charges, ...payload
    })),
    on(accountingManagementActions.InitPartner, () => ({
        ...initialState,
    })),
    on(accountingManagementActions.SelectRequester, (state: IAccountingManagementPartnerState, payload: PartnerOfAcctManagementResult) => {
        return { ...state, charges: payload.charges, ...payload };
    }),
    on(accountingManagementActions.UpdateChargeList, (state: IAccountingManagementPartnerState, payload: { charges: ChargeOfAccountingManagementModel[] }) => {
        return { ...state, charges: [...payload.charges] };
    }),
    on(accountingManagementActions.UpdateExchangeRate, (state: IAccountingManagementPartnerState, payload: accountingManagementActions.ISyncExchangeRate) => {
        if (!!state.charges.length) {
            state.charges.forEach(c => {
                c.exchangeRate = payload.exchangeRate;
            });
        }
        return {
            ...state, charges: state.charges
        };
    }),
    on(accountingManagementActions.GetAgreementForInvoice, (state: IAccountingManagementPartnerState, payload: accountingManagementActions.IAgreementInvoice) => {
        return {
            ...state, paymentTerm: payload.paymentTerm != null ? payload.paymentTerm : 30
        };
    })


);

export function reducer(state: any | undefined, action: Action) {
    return accountingManagementPartnerReducer(state, action);
}

