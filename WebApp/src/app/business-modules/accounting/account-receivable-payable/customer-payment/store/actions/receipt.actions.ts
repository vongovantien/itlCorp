import { createAction, props } from "@ngrx/store";
import { ReceiptInvoiceModel } from "@models";

export enum ReceiptActionTypes {
    INIT_INVOICE = '[AR Receipt] Init Invoice List',
    GET_INVOICE = '[AR Receipt] Get Invoice',
    GET_INVOICE_SUCCESS = '[AR Receipt] Get Invoice Success',
    REGIST_TYPE_RECEIPT = '[AR Receipt] Get Type Receipt',
    GET_INVOICE_FAIL = '[AR Receipt] Get Invoice Fail',
    GET_DEBIT_INVOICE = '[AR Receipt] Get Debit Invoice',
    RESET_INVOICE = '[AR Receipt] Reset Invoice List',


    PROCESS_CLEAR_INVOICE = '[AR Receipt] Process Clear Invoice Invoice',
    REMOVE_INVOICE = '[AR Receipt] Remove Invoice',
    REMOVE_CREDIT = '[AR Receipt] Remove Credit',
    INSERT_ADVANCE = '[AR Receipt] Insert Advance',
    SELECT_PARTNER_RECEIPT = '[AR Receipt] Select Partner',
    SELECT_DATE_RECEIPT = '[AR Receipt] Select Partner',
    SELECT_AGREEMENT = '[AR Receipt] select Agreement'


    // TODO another action receipt.
}

export const InitInvoiceList = createAction(ReceiptActionTypes.INIT_INVOICE);
export const GetInvoiceList = createAction(ReceiptActionTypes.GET_INVOICE);
export const GetInvoiceListSuccess = createAction(ReceiptActionTypes.GET_INVOICE_SUCCESS, props<{ invoices: ReceiptInvoiceModel[] }>());
export const RegistTypeReceipt = createAction(ReceiptActionTypes.REGIST_TYPE_RECEIPT, props<{ data: string, partnerId?: string }>())
export const GetInvoiceListFail = createAction(ReceiptActionTypes.GET_INVOICE_FAIL);
export const ResetInvoiceList = createAction(ReceiptActionTypes.RESET_INVOICE);

export const ProcessClearSuccess = createAction(ReceiptActionTypes.PROCESS_CLEAR_INVOICE, props<{ data: ProcessClearInvoiceModel }>())
export const RemoveInvoice = createAction(ReceiptActionTypes.REMOVE_INVOICE, props<{ index: number }>())
export const RemoveCredit = createAction(ReceiptActionTypes.REMOVE_CREDIT, props<{ index: number }>())
export const InsertAdvance = createAction(ReceiptActionTypes.INSERT_ADVANCE, props<{ data: ReceiptInvoiceModel }>());

export const SelectPartnerReceipt = createAction(ReceiptActionTypes.SELECT_PARTNER_RECEIPT, props<{ id: string, partnerGroup: string }>());
export const SelectReceiptDate = createAction(ReceiptActionTypes.SELECT_DATE_RECEIPT, props<{ date: any }>());
export const SelectReceiptAgreement = createAction(ReceiptActionTypes.SELECT_AGREEMENT, props<{ [key: string]: any; }>());
export interface ProcessClearInvoiceModel {
    invoices: ReceiptInvoiceModel[],
    cusAdvanceAmountVnd: number,
    cusAdvanceAmountUsd: number,
}