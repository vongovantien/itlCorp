import { createAction, props } from "@ngrx/store";

export enum ReceiptCombineActionTypes {
    SELECT_PARTNER_RECEIPT_COMBINE = '[AR Receipt Combine] Select Partner combine',
    UPDATE_EXCHANGE_RATE = '[AR Receipt Combine] Update Exchange Rate',
}

export const SelectPartnerReceiptCombine = createAction(ReceiptCombineActionTypes.SELECT_PARTNER_RECEIPT_COMBINE, props<{
    id: string,
    shortName: string,
    accountNo: string,
    partnerNameEn: string
}>());

export const UpdateExchangeRateReceiptCombine = createAction(ReceiptCombineActionTypes.UPDATE_EXCHANGE_RATE, props<{ exchangeRate: number }>());
