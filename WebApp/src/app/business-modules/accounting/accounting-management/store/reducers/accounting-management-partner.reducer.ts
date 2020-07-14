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
    charges: ChargeOfAccountingManagementModel[];
}

export const initialState: IAccountingManagementPartnerState = {
    partnerId: null,
    partnerName: null,
    partnerAddress: null,
    settlementRequesterId: null,
    settlementRequester: null,
    inputRefNo: null,
    charges: []
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
    })
);

export function reducer(state: any | undefined, action: Action) {
    return accountingManagementPartnerReducer(state, action);
}

