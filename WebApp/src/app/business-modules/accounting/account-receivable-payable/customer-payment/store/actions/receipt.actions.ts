import { createAction, props } from "@ngrx/store";
import { ReceiptInvoiceModel } from "@models";

export enum ReceiptActionTypes {
    INIT_INVOICE = '[AR Receipt] Init Invoice List',
    GET_INVOICE = '[AR Receipt] Get Invoice',
    GET_INVOICE_SUCCESS = '[AR Receipt] Get Invoice Success',
    GET_INVOICE_FAIL = '[AR Receipt] Get Invoice Fail',

    PROCESS_CLEAR_INVOICE = '[AR Receipt] Process Clear Invoice Invoice',
    REMOVE_INVOICE = '[AR Receipt] Remove Invoice',
    REMOVE_CREDIT = '[AR Receipt] Remove Credit',
    INSERT_ADVANCE = '[AR Receipt] Insert Advance',


    // TODO another action receipt.
}

export const InitInvoiceList = createAction(ReceiptActionTypes.INIT_INVOICE);
export const GetInvoiceList = createAction(ReceiptActionTypes.GET_INVOICE);
export const GetInvoiceListSuccess = createAction(ReceiptActionTypes.GET_INVOICE_SUCCESS, props<{ invoices: ReceiptInvoiceModel[] }>());
export const GetInvoiceListFail = createAction(ReceiptActionTypes.GET_INVOICE_FAIL);

export const ProcessClear = createAction(ReceiptActionTypes.PROCESS_CLEAR_INVOICE, props<{ amount?: number }>())
export const RemoveInvoice = createAction(ReceiptActionTypes.REMOVE_INVOICE, props<{ index: number }>())
export const RemoveCredit = createAction(ReceiptActionTypes.REMOVE_CREDIT, props<{ index: number }>())
export const InsertAdvance = createAction(ReceiptActionTypes.INSERT_ADVANCE, props<{ data: ReceiptInvoiceModel }>());
