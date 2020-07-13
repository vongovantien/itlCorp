import { createAction, props } from '@ngrx/store';
import { PartnerOfAcctManagementResult, ChargeOfAccountingManagementModel } from '@models';

export enum AccountingManagementActionTypes {
    SELECT_PARTNER = '[Accounting Management] Select Partner',
    INIT_PARTNER = '[Accounting Management] Init Partner',
    SELECT_REQUESTER = '[Accounting Management] Select Requester',
    UPDATE_CHARGE_LIST = '[Accounting Management] Update Charge List',
    UPDATE_EXCHANGE_RATE = '[Accounting Management] Update Exchange Rate',
}

export interface ISyncExchangeRate {
    exchangeRate: number;
}

export const SelectPartner = createAction(AccountingManagementActionTypes.SELECT_PARTNER, props<PartnerOfAcctManagementResult>());
export const InitPartner = createAction(AccountingManagementActionTypes.INIT_PARTNER);
export const SelectRequester = createAction(AccountingManagementActionTypes.SELECT_REQUESTER, props<PartnerOfAcctManagementResult>());
export const UpdateChargeList = createAction(AccountingManagementActionTypes.UPDATE_CHARGE_LIST, props<{ charges: ChargeOfAccountingManagementModel[] }>());
export const UpdateExchangeRate = createAction(AccountingManagementActionTypes.UPDATE_EXCHANGE_RATE, props<ISyncExchangeRate>());

