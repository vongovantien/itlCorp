import { createAction, props } from '@ngrx/store';
import { PartnerOfAcctManagementResult } from '@models';

export enum AccountingManagementActionTypes {
    SELECT_PARTNER = '[Accounting Management] Select Partner',
    INIT_PARTNER = '[Accounting Management] Init Partner',
    SELECT_REQUESTER = '[Accounting Management] Select Requester',
}

export const SelectPartner = createAction(AccountingManagementActionTypes.SELECT_PARTNER, props<PartnerOfAcctManagementResult>());
export const InitPartner = createAction(AccountingManagementActionTypes.INIT_PARTNER);
export const SelectRequester = createAction(AccountingManagementActionTypes.SELECT_REQUESTER, props<PartnerOfAcctManagementResult>());

