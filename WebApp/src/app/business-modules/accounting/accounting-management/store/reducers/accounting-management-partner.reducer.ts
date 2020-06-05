import { createReducer, on, Action } from '@ngrx/store';
import * as accountingManagementActions from '../actions/accounting-management.action';
import { ChargeOfAccountingManagementModel, PartnerOfAcctManagementResult } from '@models';

export interface IAccountingManagementPartnerState {
    partnerId: string;
    partnerName: string;
    partnerAddress: string;
    settlementRequester: string;
    inputRefNo: string;
    charges: ChargeOfAccountingManagementModel[];
}

export const initialState: IAccountingManagementPartnerState = {
    partnerId: null,
    partnerName: null,
    partnerAddress: null,
    settlementRequester: null,
    inputRefNo: null,
    charges: []
};

export const accountingManagementPartnerReducer = createReducer(initialState, on(accountingManagementActions.SelectPartner,
    (state: IAccountingManagementPartnerState, payload: PartnerOfAcctManagementResult) => ({
        ...state, charges: payload.charges, ...payload
    })));

